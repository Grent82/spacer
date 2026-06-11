namespace Spacer.Application.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using Spacer.Application.Ports;
using Spacer.Domain.Entities;
using Spacer.Domain.ValueObjects;

/// <summary>
/// Calculates fleet posture for a ruler based on their fleets' positions and destinations.
/// </summary>
public sealed class FleetPostureProvider
{
    private readonly FleetRepository _fleetRepository;
    private readonly Lazy<Dictionary<EntityId, EntityId>> _planetOwners;

    public FleetPostureProvider(FleetRepository fleetRepository, IReadOnlyList<Planet> planets)
    {
        _fleetRepository = fleetRepository;
        // Build a map of planet position -> owner faction id
        _planetOwners = new Lazy<Dictionary<EntityId, EntityId>>(() =>
        {
            var map = new Dictionary<EntityId, EntityId>();
            foreach (var planet in planets)
            {
                // Use planet id as the key for position matching
                map[planet.Id] = planet.OwnerFactionId;
            }
            return map;
        });
    }

    public FleetPostureSummary GetForRuler(EntityId rulerId, IReadOnlyList<Planet> allPlanets)
    {
        var fleets = _fleetRepository.GetByOverlord(rulerId);
        if (fleets.Count == 0)
        {
            return FleetPostureSummary.Empty;
        }

        var totalFleets = fleets.Count;
        var fleetsMovingToOwn = 0;
        var fleetsMovingToEnemy = 0;

        // Get the ruler's faction id (ruler id typically equals faction id for leaders)
        var rulerFactionId = rulerId;

        foreach (var fleet in fleets)
        {
            if (fleet.Destination == fleet.Position)
            {
                // Fleet is not moving
                continue;
            }

            // Check if destination is owned by same faction
            var destinationOwner = GetPlanetOwner(fleet.Destination, allPlanets);
            if (destinationOwner == rulerFactionId)
            {
                fleetsMovingToOwn++;
            }
            else if (!destinationOwner.IsNone)
            {
                // Destination is owned by a different faction
                fleetsMovingToEnemy++;
            }
        }

        return new FleetPostureSummary(totalFleets, fleetsMovingToOwn, fleetsMovingToEnemy);
    }

    private EntityId GetPlanetOwner(Position position, IReadOnlyList<Planet> planets)
    {
        // Find planet at this position and return its owner
        // Note: This assumes position can be matched to planet id
        // TODO: Improve when planet position tracking is implemented
        foreach (var planet in planets)
        {
            if (planet.Position == position)
            {
                return planet.OwnerFactionId;
            }
        }
        return EntityId.None;
    }
}

/// <summary>
/// Simplified posture provider for use with EconomyTurnService.
/// </summary>
public sealed class SimpleFleetPostureProvider : IFleetPostureProvider
{
    private readonly FleetRepository _fleetRepository;
    private readonly IReadOnlyList<Planet> _planets;

    public SimpleFleetPostureProvider(FleetRepository fleetRepository, IReadOnlyList<Planet> planets)
    {
        _fleetRepository = fleetRepository;
        _planets = planets;
    }

    public FleetPostureSummary GetForRuler(EntityId rulerId)
    {
        var postureProvider = new FleetPostureProvider(_fleetRepository, _planets);
        return postureProvider.GetForRuler(rulerId, _planets);
    }
}
