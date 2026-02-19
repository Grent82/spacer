namespace Spacer.Domain.Entities;

using System;
using Spacer.Domain.ValueObjects;

public sealed class WeaponSpec
{
    private WeaponSpec(EntityId id, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name must be provided.", nameof(name));
        }

        Id = id;
        Name = name;
    }

    public static WeaponSpec Create(EntityId id, string name) => new(id, name);
    public static WeaponSpec Create(int id, string name) => new(EntityId.Create(id), name);

    public EntityId Id { get; }
    public string Name { get; private set; }

    public int ShipTypeCode { get; private set; }
    public int FactionId { get; private set; }
    public int Tier { get; private set; }
    public int DpmId { get; private set; }
    public string ImageKey { get; private set; } = string.Empty;

    public void ConfigureDefinition(
        int shipTypeCode,
        int factionId,
        int tier,
        int dpmId,
        string imageKey
    )
    {
        if (shipTypeCode <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(shipTypeCode));
        }
        if (factionId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(factionId));
        }
        if (tier <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tier));
        }
        if (dpmId < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(dpmId));
        }

        ShipTypeCode = shipTypeCode;
        FactionId = factionId;
        Tier = tier;
        DpmId = dpmId;
        ImageKey = imageKey ?? string.Empty;
    }
}
