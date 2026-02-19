namespace Spacer.Application.Services;

using System;
using Spacer.Application.Config;
using Spacer.Application.DTOs;
using Spacer.Application.Ports;
using Spacer.Domain.Entities;
using Spacer.Domain.Enums;
using Spacer.Domain.Services;
using Spacer.Domain.ValueObjects;

public sealed class FleetSpecRebuildService
{
    private const int FleetShipTypeMin = 100;
    private const int FleetShipTypeMaxExclusive = 190;
    private const int StarfighterTypeMin = 200;
    private const int StarfighterTypeMaxExclusive = 300;
    private const int GroundUnitTypeMin = 300;
    private const int GroundUnitTypeMaxExclusive = 400;
    private const int FleetSpecialTypeCode = 400;

    private readonly IShipSpecCatalog _shipSpecCatalog;
    private readonly IWeaponIdResolver _weaponIdResolver;
    private readonly IPlanetFleetSpecStore _specStore;
    private readonly PlanetResearchService _planetResearchService;
    private readonly GameConfig _config;

    public FleetSpecRebuildService( IShipSpecCatalog shipSpecCatalog, IWeaponIdResolver weaponIdResolver, IPlanetFleetSpecStore specStore, PlanetResearchService planetResearchService, GameConfig config )
    {
        _shipSpecCatalog = shipSpecCatalog;
        _weaponIdResolver = weaponIdResolver;
        _specStore = specStore;
        _planetResearchService = planetResearchService;
        _config = config;
    }

    public PlanetFleetSpecSet Rebuild(Planet planet, WeaponResearchCategory category)
    {
        if (planet.OwnerFactionId.IsNone)
        {
            return new PlanetFleetSpecSet( planet.Id, category, Array.Empty<ShipSlotSpec>() );
        }
        var factionId = planet.OwnerFactionId.Value;
        var remaining = planet.Research.GetRemainingWeaponResearch((int)category);
        var tier = _planetResearchService.ComputeWeaponReleaseStage(
            _config.WeaponReleaseChanges,
            remaining
        );

        var shipSpecs = _shipSpecCatalog.GetAll();
        var slots = new List<ShipSlotSpec>(shipSpecs.Count);

        for (var i = 0; i < shipSpecs.Count; i++)
        {
            var spec = shipSpecs[i];
            var typeCode = spec.TypeCode;

            if (!IsTypeInCategory(typeCode, category))
            {
                continue;
            }

            var weaponId = _weaponIdResolver.ResolveWeaponId(factionId, tier, typeCode);
            var optimized = BuildOptimizedStats(planet, spec, category);
            var cost = ComputeCost(spec.BaseCost, tier, category, typeCode);
            var loadings = ComputeLoadings(spec, category, typeCode);

            var slotIndex = spec.CatalogIndex;
            if (slotIndex < 0)
            {
                slotIndex = i;
            }

            slots.Add( new ShipSlotSpec( slotIndex, spec.Id, typeCode, weaponId, optimized, cost, loadings.Carrier, loadings.LandingPod, loadings.Ship ) );
        }

        var specSet = new PlanetFleetSpecSet(planet.Id, category, slots);
        _specStore.Save(specSet);
        return specSet;
    }

    private static bool IsTypeInCategory(int typeCode, WeaponResearchCategory category)
    {
        return category switch
        {
            WeaponResearchCategory.FleetShips => typeCode == FleetSpecialTypeCode
                || (typeCode >= FleetShipTypeMin && typeCode < FleetShipTypeMaxExclusive),
            WeaponResearchCategory.Starfighters => typeCode >= StarfighterTypeMin
                && typeCode < StarfighterTypeMaxExclusive,
            WeaponResearchCategory.GroundUnits => typeCode >= GroundUnitTypeMin
                && typeCode < GroundUnitTypeMaxExclusive,
            _ => false
        };
    }

    private static ShipStatBlock BuildOptimizedStats( Planet planet, ShipSpec spec, WeaponResearchCategory category )
    {
        var baseStats = spec.BaseStats.ToArray();
        var optimized = new int[ShipStatBlock.StatCount];
        var applyFleetBoost = category == WeaponResearchCategory.FleetShips
            && spec.TypeCode != FleetSpecialTypeCode;

        for (var i = 0; i < ShipStatBlock.StatCount; i++)
        {
            var researchFactor = planet.Research.GetCurrentLevel(i) / 1000;
            if (applyFleetBoost
                && (i == (int)ShipStatIndex.Minesweeping
                    || i == (int)ShipStatIndex.Construction
                    || i == (int)ShipStatIndex.Maneuvering))
            {
                optimized[i] = baseStats[i] + researchFactor * 3;
                continue;
            }

            var multiplier = baseStats[i] / 2;
            if (multiplier == 0)
            {
                multiplier = 1;
            }

            optimized[i] = baseStats[i] + researchFactor * multiplier;
        }

        return new ShipStatBlock(optimized);
    }

    private static int ComputeCost(
        int baseCost,
        int tier,
        WeaponResearchCategory category,
        int typeCode
    )
    {
        if (category == WeaponResearchCategory.FleetShips && typeCode == FleetSpecialTypeCode)
        {
            return baseCost;
        }

        var multiplier = tier - 1;
        if (multiplier < 0)
        {
            multiplier = 0;
        }

        return baseCost + baseCost * multiplier / 2;
    }

    private static (int Carrier, int LandingPod, int Ship) ComputeLoadings( ShipSpec spec, WeaponResearchCategory category, int typeCode )
    {
        if (category != WeaponResearchCategory.FleetShips || typeCode == FleetSpecialTypeCode)
        {
            return (0, 0, 0);
        }

        return (
            spec.BaseCarrierLoading > 0 ? spec.BaseCarrierLoading : 0,
            spec.BaseLandingPodLoading > 0 ? spec.BaseLandingPodLoading : 0,
            spec.BaseShipLoading > 0 ? spec.BaseShipLoading : 0
        );
    }
}
