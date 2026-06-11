namespace Spacer.Domain.Entities;

using System;
using System.Collections.Generic;
using System.Linq;
using Spacer.Domain.ValueObjects;

/// <summary>
/// In-memory repository for fleet instances.
/// TODO: Replace with persistent storage when fleet CSV files are created.
/// </summary>
public sealed class FleetRepository
{
    private readonly Dictionary<EntityId, Fleet> _fleets = new();

    public IReadOnlyList<Fleet> GetAll() => _fleets.Values.ToList();

    public Fleet? FindById(EntityId id)
    {
        return _fleets.TryGetValue(id, out var fleet) ? fleet : null;
    }

    public void Add(Fleet fleet)
    {
        _fleets[fleet.Id] = fleet;
    }

    public void Remove(EntityId id)
    {
        _fleets.Remove(id);
    }

    public IReadOnlyList<Fleet> GetByOverlord(EntityId overlordId)
    {
        return _fleets.Values.Where(f => f.OverlordId == overlordId).ToList();
    }

    public IReadOnlyList<Fleet> GetByPosition(Position position)
    {
        return _fleets.Values.Where(f => f.Position == position).ToList();
    }
}
