namespace Spacer.Domain.Entities;

using System;
using Spacer.Domain.Enums;
using Spacer.Domain.ValueObjects;

public sealed class Faction
{
    private Faction(EntityId id, string name, FactionType type)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name must be provided.", nameof(name));
        }

        Id = id;
        Name = name;
        Type = type;
    }

    public static Faction Create(EntityId id, string name, FactionType type) => new(id, name, type);
    public static Faction Create(int id, string name, FactionType type) => new(EntityId.Create(id), name, type);

    public EntityId Id { get; }
    public string Name { get; private set; }
    public FactionType Type { get; private set; }

    public EntityId CapitalPlanetId { get; private set; } = EntityId.None;
    public EntityId OverlordId { get; private set; } = EntityId.None;

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name must be provided.", nameof(name));
        }

        Name = name;
    }
}
