namespace Spacer.Domain.Entities;

using System;
using Spacer.Domain.ValueObjects;

public sealed class ShipSpec
{
    private ShipSpec(EntityId id, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name must be provided.", nameof(name));
        }

        Id = id;
        Name = name;
    }

    public static ShipSpec Create(EntityId id, string name) => new(id, name);
    public static ShipSpec Create(int id, string name) => new(EntityId.Create(id), name);

    public EntityId Id { get; }
    public string Name { get; private set; }
    public string ImageKey { get; private set; } = string.Empty;

    public int Variant { get; private set; }
    public EntityId TypeId { get; private set; } = EntityId.None;

    public int Attack { get; private set; }
    public int AntiAir { get; private set; }
    public int Defense { get; private set; }
    public int Minesweeping { get; private set; }
    public int Construction { get; private set; }

    public int ShipLoading { get; private set; }
    public int CarrierLoading { get; private set; }
    public int LandingPodLoading { get; private set; }

    public int Cost { get; private set; }
}
