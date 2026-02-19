namespace Spacer.Application.Services;

using Spacer.Application.Config;
using Spacer.Application.DTOs;
using Spacer.Application.Ports;
using Spacer.Domain.Entities;
using Spacer.Domain.Enums;
using Spacer.Domain.Services;
using Spacer.Domain.ValueObjects;

public sealed class WeaponResearchProgressionService
{
    private const int WeaponReleaseJudgeCategory = 9;

    private readonly PlanetResearchService _planetResearchService;
    private readonly IJudgementLookup _judgementLookup;
    private readonly IFleetSpecRebuilder _fleetSpecRebuilder;
    private readonly PlanetResearchRules _rules;
    private readonly int _weaponReleaseChanges;

    public WeaponResearchProgressionService( PlanetResearchService planetResearchService, IJudgementLookup judgementLookup, IFleetSpecRebuilder fleetSpecRebuilder, GameConfig config )
        : this( planetResearchService, judgementLookup, fleetSpecRebuilder, config.PlanetResearchRules, config.WeaponReleaseChanges )
    {
    }

    public WeaponResearchProgressionService( PlanetResearchService planetResearchService, IJudgementLookup judgementLookup, IFleetSpecRebuilder fleetSpecRebuilder, PlanetResearchRules rules, int weaponReleaseChanges )
    {
        _planetResearchService = planetResearchService;
        _judgementLookup = judgementLookup;
        _fleetSpecRebuilder = fleetSpecRebuilder;
        _rules = rules;
        _weaponReleaseChanges = weaponReleaseChanges;
    }

    public void InitializeWeaponRelease(Planet planet)
    {
        _planetResearchService.InitializeWeaponReleaseRemaining( planet, _weaponReleaseChanges, _rules );
    }

    public bool TryUnlockWeaponRelease(Planet planet, PersonalityType personality)
    {
        var requiredDelta = _judgementLookup.GetJudgementValue( WeaponReleaseJudgeCategory, personality );
        return _planetResearchService.TryMarkWeaponReleaseUpdated(planet, requiredDelta);
    }

    public bool ApplyWeaponRelease(Planet planet, WeaponResearchCategory category)
    {
        var changed = _planetResearchService.ApplyWeaponRelease( planet, (int)category, _rules );

        if (changed)
        {
            _fleetSpecRebuilder.RebuildFleetSpecs(planet, category);
        }

        return changed;
    }

    public bool ApplyWeaponReleaseForAllCategories(Planet planet)
    {
        var changed = _planetResearchService.ApplyWeaponReleaseForAllCategories(planet, _rules);
        if (!changed)
        {
            return false;
        }

        _fleetSpecRebuilder.RebuildFleetSpecs(planet, WeaponResearchCategory.FleetShips);
        _fleetSpecRebuilder.RebuildFleetSpecs(planet, WeaponResearchCategory.Starfighters);
        _fleetSpecRebuilder.RebuildFleetSpecs(planet, WeaponResearchCategory.GroundUnits);
        return true;
    }
}
