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
}
