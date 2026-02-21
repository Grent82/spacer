namespace Spacer.Application.UseCases;

using Spacer.Application.Config;
using Spacer.Application.Ports;
using Spacer.Application.Services;
using Spacer.Domain.Entities;
using Spacer.Domain.ValueObjects;

public sealed class RunTurnUseCase
{
    private readonly IPlanetRepository _planets;
    private readonly ICharacterRepository _characters;
    private readonly IFleetPostureProvider _fleetPostureProvider;
    private readonly EconomyTurnService _economyTurnService;
    private readonly WeaponResearchProgressionService _researchService;
    private readonly FactionPoliticsTurnService _factionPoliticsService;
    private readonly GameConfig _config;

    public RunTurnUseCase(
        IPlanetRepository planets,
        ICharacterRepository characters,
        IFleetPostureProvider fleetPostureProvider,
        EconomyTurnService economyTurnService,
        WeaponResearchProgressionService researchService,
        FactionPoliticsTurnService factionPoliticsService,
        GameConfig config
    )
    {
        _planets = planets;
        _characters = characters;
        _fleetPostureProvider = fleetPostureProvider;
        _economyTurnService = economyTurnService;
        _researchService = researchService;
        _factionPoliticsService = factionPoliticsService;
        _config = config;
    }

    public void Execute()
    {
        // 1) Reset per-turn flags.
        foreach (var planet in _planets.GetAll())
        {
            planet.ResetTurnFlags();
        }

        // 2) Economy tick (salary income, opinion drift, loyalty impact, population growth).
        var allPlanets = _planets.GetAll();
        _economyTurnService.ApplyPlanetEconomy(
            allPlanets,
            _config.GameRules,
            _config.PlanetEconomyRules,
            planet => planet.RulerId.IsNone
                ? FleetPostureSummary.Empty
                : _fleetPostureProvider.GetForRuler(planet.RulerId)
        );

        // 3) Research progression (weapon release unlocks).
        foreach (var planet in allPlanets)
        {
            if (planet.RulerId.IsNone)
            {
                continue;
            }

            var ruler = _characters.FindById(planet.RulerId);
            if (ruler is null)
            {
                continue;
            }

            _researchService.TryUnlockWeaponRelease(planet, ruler.Personality);
        }

        // 4) Faction politics (defection/joining).
        _factionPoliticsService.Apply(_config.GameRules, _config.FactionPoliticsRules, _config.PlanetEconomyRules);

        // 5) Additional per-turn systems (diplomacy, battles, events) TBD.
    }
}
