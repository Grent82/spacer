namespace Spacer.Domain.Services;

using Spacer.Domain.Entities;
using Spacer.Domain.ValueObjects;

public sealed class PlanetEconomyService
{
    public int ComputeSalary(Planet planet, GameRules rules)
    {
        var salary = planet.Population * planet.CitizensLoyalty / 80 + planet.Population / 5;

        if (!planet.FeudalLordId.IsNone)
        {
            salary /= 3;
        }

        if (rules.EasyMode && planet.RulerId == rules.PlayerOverlordId)
        {
            salary *= 2;
            if (!planet.FeudalLordId.IsNone)
            {
                salary /= 2;
            }
        }

        return salary;
    }

    public void AdjustPopulation(Planet planet, int delta, PlanetEconomyRules rules)
    {
        planet.AdjustPopulation(delta, rules);
    }

    public void AdjustPublicOpinion(Planet planet, int delta, PlanetEconomyRules rules)
    {
        planet.AdjustPublicOpinion(delta, rules);
    }

    public void AdjustCitizensLoyalty(Planet planet, int delta, PlanetEconomyRules rules)
    {
        planet.AdjustCitizensLoyalty(delta, rules);
    }

    public void AdjustGold(Planet planet, int delta, PlanetEconomyRules rules)
    {
        planet.AdjustGold(delta, rules);
    }

    /// <summary>
    /// Computes production for a planet based on population, loyalty, public opinion, and research.
    /// </summary>
    public int ComputeProduction(Planet planet, PlanetEconomyRules rules)
    {
        var productionRules = rules.Production;

        // Base production from population.
        var baseProduction = planet.Population * productionRules.BaseProductionPerCitizen / 1000;

        // Loyalty bonus (0-100% based on citizens loyalty).
        var loyaltyBonus = baseProduction * productionRules.LoyaltyBonusPercent * planet.CitizensLoyalty / 10000;

        // Public opinion bonus (0-30% based on public opinion).
        var publicOpinionBonus = baseProduction * productionRules.PublicOpinionBonusPercent * planet.PublicOpinion / 10000;

        // Research efficiency bonus (up to 20% based on research level).
        var researchBonus = baseProduction * productionRules.ResearchEfficiencyBonusPercent * planet.Research.SystemTechLevel / 1000;

        var totalProduction = baseProduction + loyaltyBonus + publicOpinionBonus + researchBonus;

        // Apply decay if production is too high (prevents runaway growth).
        var currentProduction = planet.Production;
        if (currentProduction > 0 && totalProduction > currentProduction * 150 / 100)
        {
            var decay = currentProduction * productionRules.ProductionDecayPercent / 100;
            totalProduction = Math.Min(totalProduction, currentProduction + decay);
        }

        return Math.Max(0, totalProduction);
    }

    /// <summary>
    /// Adjusts production with decay protection.
    /// </summary>
    public void AdjustProduction(Planet planet, int delta, PlanetEconomyRules rules)
    {
        planet.AdjustProduction(delta, rules);
    }

    /// <summary>
    /// Computes population growth cost in production units.
    /// </summary>
    public int ComputePopulationGrowthCost(int populationDelta, PlanetEconomyRules rules)
    {
        if (populationDelta <= 0)
        {
            return 0;
        }

        return populationDelta * rules.Production.PopulationGrowthCostDivisor;
    }

    /// <summary>
    /// Deducts production cost for population growth.
    /// </summary>
    public bool TrySpendProductionForPopulationGrowth(Planet planet, int populationDelta, PlanetEconomyRules rules)
    {
        var cost = ComputePopulationGrowthCost(populationDelta, rules);
        if (planet.Production < cost)
        {
            return false;
        }

        planet.AdjustProduction(-cost, rules);
        return true;
    }
}
