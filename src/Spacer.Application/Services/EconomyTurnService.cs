namespace Spacer.Application.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using Spacer.Domain.Entities;
using Spacer.Domain.Services;
using Spacer.Domain.ValueObjects;

public sealed class EconomyTurnService
{
    private readonly PlanetEconomyService _planetEconomyService;
    private readonly IRandomSource _random;

    public EconomyTurnService(PlanetEconomyService planetEconomyService)
        : this(planetEconomyService, new SystemRandomSource())
    {
    }

    public EconomyTurnService(PlanetEconomyService planetEconomyService, IRandomSource random)
    {
        _planetEconomyService = planetEconomyService;
        _random = random;
    }

    public void ApplyPlanetEconomy( IReadOnlyList<Planet> planets, GameRules gameRules, PlanetEconomyRules economyRules, Func<Planet, FleetPostureSummary> postureProvider )
    {
        if (planets.Count == 0)
        {
            return;
        }

        var planetsByFaction = BuildPlanetsByFaction(planets);
        if (planetsByFaction.Count == 0)
        {
            return;
        }

        var capitalsByFaction = ResolveCapitals(planetsByFaction);

        ApplyPublicOpinionDrift(capitalsByFaction.Values, economyRules);
        ApplySalaryIncome(planetsByFaction, capitalsByFaction, gameRules, economyRules);
        ApplyLoyaltyDriftFromPosture(planetsByFaction, capitalsByFaction, economyRules, postureProvider);
        ApplyPopulationGrowth(planetsByFaction, economyRules);
    }

    private void ApplyPublicOpinionDrift(IEnumerable<Planet> capitals, PlanetEconomyRules rules)
    {
        foreach (var capital in capitals)
        {
            var opinionDelta = ComputePublicOpinionDrift(capital, rules);
            if (opinionDelta != 0)
            {
                _planetEconomyService.AdjustPublicOpinion(capital, opinionDelta, rules);
            }
        }
    }

    private void ApplySalaryIncome(
        IReadOnlyDictionary<EntityId, List<Planet>> planetsByFaction,
        IReadOnlyDictionary<EntityId, Planet> capitalsByFaction,
        GameRules gameRules,
        PlanetEconomyRules rules
    )
    {
        if (!gameRules.IsSalaryMonth)
        {
            return;
        }

        foreach (var entry in planetsByFaction)
        {
            var factionId = entry.Key;
            if (!capitalsByFaction.TryGetValue(factionId, out var capital))
            {
                continue;
            }

            foreach (var planet in entry.Value)
            {
                var salaryIncome = _planetEconomyService.ComputeSalary(planet, gameRules);
                if (salaryIncome <= 0)
                {
                    continue;
                }

                _planetEconomyService.AdjustGold(capital, salaryIncome, rules);
            }
        }
    }

    private void ApplyLoyaltyDriftFromPosture(
        IReadOnlyDictionary<EntityId, List<Planet>> planetsByFaction,
        IReadOnlyDictionary<EntityId, Planet> capitalsByFaction,
        PlanetEconomyRules rules,
        Func<Planet, FleetPostureSummary> postureProvider
    )
    {
        foreach (var entry in capitalsByFaction)
        {
            var capital = entry.Value;
            var posture = postureProvider(capital);
            var loyaltyDelta = ComputeLoyaltyDriftFromPosture(capital, posture, rules);
            if (loyaltyDelta == 0)
            {
                continue;
            }

            if (!planetsByFaction.TryGetValue(entry.Key, out var factionPlanets))
            {
                continue;
            }

            foreach (var planet in factionPlanets)
            {
                _planetEconomyService.AdjustCitizensLoyalty(planet, loyaltyDelta, rules);
            }
        }
    }

    private void ApplyPopulationGrowth(IReadOnlyDictionary<EntityId, List<Planet>> planetsByFaction, PlanetEconomyRules rules)
    {
        foreach (var entry in planetsByFaction)
        {
            foreach (var planet in entry.Value)
            {
                var populationDelta = ComputePopulationGrowth(planet, rules);
                if (populationDelta != 0)
                {
                    _planetEconomyService.AdjustPopulation(planet, populationDelta, rules);
                }

                ApplyLowLoyaltyPopulationCrash(planet, rules);
            }
        }
    }

    private void ApplyLowLoyaltyPopulationCrash(Planet planet, PlanetEconomyRules rules)
    {
        if (planet.CitizensLoyalty >= rules.LowLoyaltyPopulationThreshold)
        {
            return;
        }

        if (rules.LowLoyaltyPopulationCrashOdds <= 0)
        {
            return;
        }

        var roll = _random.Next(rules.LowLoyaltyPopulationCrashOdds);
        if (roll != rules.LowLoyaltyPopulationCrashTrigger)
        {
            return;
        }

        if (rules.LowLoyaltyPopulationCrashDivisor <= 0)
        {
            return;
        }

        var crash = planet.Population / rules.LowLoyaltyPopulationCrashDivisor;
        if (crash > 0)
        {
            _planetEconomyService.AdjustPopulation(planet, -crash, rules);
        }
        var loyaltyDelta = rules.LowLoyaltyResetValue - planet.CitizensLoyalty;
        _planetEconomyService.AdjustCitizensLoyalty(planet, loyaltyDelta, rules);
    }

    private static int ComputePublicOpinionDrift(Planet planet, PlanetEconomyRules rules)
    {
        if (planet.PublicOpinion > rules.PublicOpinionHighThreshold)
        {
            return -rules.PublicOpinionDriftPerTurn;
        }
        if (planet.PublicOpinion < rules.PublicOpinionLowThreshold)
        {
            return rules.PublicOpinionDriftPerTurn;
        }

        return 0;
    }

    private static int ComputePopulationGrowth(Planet planet, PlanetEconomyRules rules)
    {
        if (rules.PopulationGrowthDivisor <= 0)
        {
            return 0;
        }

        return planet.Population / rules.PopulationGrowthDivisor;
    }

    private static int ComputeLoyaltyDriftFromPosture(Planet planet, FleetPostureSummary posture, PlanetEconomyRules rules)
    {
        if (!planet.IsCapitalCity)
        {
            return 0;
        }

        if (planet.PublicOpinion < rules.PublicOpinionLowThreshold)
        {
            if (posture.TotalFleets >= rules.FleetPostureMinFleets && posture.FleetsMovingToOwn > posture.TotalFleets / 2)
            {
                return -rules.FleetPostureOwnPenalty;
            }
        }
        else if (planet.PublicOpinion > rules.PublicOpinionHighThreshold)
        {
            if (posture.FleetsMovingToEnemy > 0)
            {
                return -rules.FleetPostureEnemyPenalty;
            }
        }

        return 0;
    }

    private static Dictionary<EntityId, List<Planet>> BuildPlanetsByFaction(IReadOnlyList<Planet> planets)
    {
        var result = new Dictionary<EntityId, List<Planet>>();
        foreach (var planet in planets)
        {
            if (planet.OwnerFactionId.IsNone)
            {
                continue;
            }

            if (!result.TryGetValue(planet.OwnerFactionId, out var list))
            {
                list = new List<Planet>();
                result.Add(planet.OwnerFactionId, list);
            }

            list.Add(planet);
        }

        return result;
    }

    private static Dictionary<EntityId, Planet> ResolveCapitals(IReadOnlyDictionary<EntityId, List<Planet>> planetsByFaction)
    {
        var result = new Dictionary<EntityId, Planet>();
        foreach (var entry in planetsByFaction)
        {
            var capital = entry.Value.FirstOrDefault(planet => planet.IsCapitalCity) ?? entry.Value[0];
            result[entry.Key] = capital;
        }

        return result;
    }
}
