namespace Spacer.Infrastructure.Persistence;

using System.Collections.Generic;
using Spacer.Application.DTOs;
using Spacer.Application.Ports;
using Spacer.Domain.ValueObjects;

public sealed class InMemoryPlanetFleetSpecStore : IPlanetFleetSpecStore
{
    private readonly Dictionary<PlanetSpecKey, PlanetFleetSpecSet> _store = new();

    public void Save(PlanetFleetSpecSet specSet)
    {
        var key = new PlanetSpecKey(specSet.PlanetId, specSet.Category);
        _store[key] = specSet;
    }

    public bool TryGet(EntityId planetId, WeaponResearchCategory category, out PlanetFleetSpecSet specSet)
    {
        return _store.TryGetValue(new PlanetSpecKey(planetId, category), out specSet);
    }
}

public readonly record struct PlanetSpecKey(EntityId PlanetId, WeaponResearchCategory Category);
