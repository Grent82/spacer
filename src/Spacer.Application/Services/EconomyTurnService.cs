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

    public EconomyTurnResult ApplyPlanetEconomy( Planet planet, GameRules gameRules, PlanetEconomyRules economyRules )
    {
        var productionIncome = Math.Max(0, planet.Production);
        var salaryIncome = Math.Max(0, _planetEconomyService.ComputeSalary(planet, gameRules));
        var totalIncome = productionIncome + salaryIncome;

        _planetEconomyService.AdjustGold(planet, totalIncome, economyRules);

        var opinionDelta = ComputePublicOpinionDrift(planet);
        if (opinionDelta != 0)
        {
            _planetEconomyService.AdjustPublicOpinion(planet, opinionDelta, economyRules);
        }

        return new EconomyTurnResult(productionIncome, salaryIncome, opinionDelta);
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
}
