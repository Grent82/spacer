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
    private readonly GameConfig _config;

    public RunTurnUseCase(
        IPlanetRepository planets,
        ICharacterRepository characters,
        IFleetPostureProvider fleetPostureProvider,
        EconomyTurnService economyTurnService,
        WeaponResearchProgressionService researchService,
        GameConfig config
    )
    {
        _planets = planets;
        _characters = characters;
        _fleetPostureProvider = fleetPostureProvider;
        _economyTurnService = economyTurnService;
        _researchService = researchService;
        _config = config;
    }

    public void Execute()
    {
        // 1) Reset per-turn flags.
        foreach (var planet in _planets.GetAll())
        {
            planet.ResetTurnFlags();
        }

        // 2) Economy tick (production -> gold -> opinion drift).
        foreach (var planet in _planets.GetAll())
        {
            if (planet.OwnerFactionId.IsNone)
            {
                continue;
            }

            var posture = planet.RulerId.IsNone
                ? FleetPostureSummary.Empty
                : _fleetPostureProvider.GetForRuler(planet.RulerId);

            _economyTurnService.ApplyPlanetEconomy(
                planet,
                _config.GameRules,
                _config.PlanetEconomyRules,
                posture
            );
        }

        // 3) Research progression (weapon release unlocks).
        foreach (var planet in _planets.GetAll())
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

        // 4) Additional per-turn systems (diplomacy, battles, events) TBD.
    }
}
