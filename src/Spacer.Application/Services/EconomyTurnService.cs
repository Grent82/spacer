namespace Spacer.Application.Services;

using Spacer.Domain.Entities;
using Spacer.Domain.Services;
using Spacer.Domain.ValueObjects;

public sealed class EconomyTurnService
{
    private readonly PlanetEconomyService _planetEconomyService;

    public EconomyTurnService(PlanetEconomyService planetEconomyService)
    {
        _planetEconomyService = planetEconomyService;
    }

    public EconomyTurnResult ApplyPlanetEconomy(
        Planet planet,
        GameRules gameRules,
        PlanetEconomyRules economyRules,
        FleetPostureSummary posture
    )
    {
        var productionIncome = Math.Max(0, planet.Production);
        var salaryIncome = gameRules.IsSalaryMonth
            ? Math.Max(0, _planetEconomyService.ComputeSalary(planet, gameRules))
            : 0;
        var totalIncome = productionIncome + salaryIncome;

        _planetEconomyService.AdjustGold(planet, totalIncome, economyRules);

        var opinionDelta = ComputePublicOpinionDrift(planet);
        if (opinionDelta != 0)
        {
            _planetEconomyService.AdjustPublicOpinion(planet, opinionDelta, economyRules);
        }

        var loyaltyDelta = ComputeLoyaltyDriftFromPosture(planet, posture);
        if (loyaltyDelta != 0)
        {
            _planetEconomyService.AdjustCitizensLoyalty(planet, loyaltyDelta, economyRules);
        }

        var populationDelta = ComputePopulationGrowth(planet, economyRules);
        if (populationDelta != 0)
        {
            _planetEconomyService.AdjustPopulation(planet, populationDelta, economyRules);
        }

        return new EconomyTurnResult(
            productionIncome,
            salaryIncome,
            opinionDelta,
            populationDelta,
            loyaltyDelta
        );
    }

    private static int ComputePublicOpinionDrift(Planet planet)
    {
        if (planet.PublicOpinion > 55)
        {
            return -1;
        }
        if (planet.PublicOpinion < 45)
        {
            return 1;
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

    private static int ComputeLoyaltyDriftFromPosture(Planet planet, FleetPostureSummary posture)
    {
        if (!planet.IsCapitalCity)
        {
            return 0;
        }

        if (planet.PublicOpinion < 45)
        {
            if (posture.TotalFleets >= 2 && posture.FleetsMovingToOwn > posture.TotalFleets / 2)
            {
                return -3;
            }
        }
        else if (planet.PublicOpinion > 55)
        {
            if (posture.FleetsMovingToEnemy > 0)
            {
                return -4;
            }
        }

        return 0;
    }
}
