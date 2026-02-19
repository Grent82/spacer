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

    public int CatalogIndex { get; private set; }
    public int TypeCode { get; private set; }
    public ShipStatBlock BaseStats { get; private set; } = ShipStatBlock.Empty;

    public int BaseCost { get; private set; }
    public int BaseCarrierLoading { get; private set; }
    public int BaseLandingPodLoading { get; private set; }
    public int BaseShipLoading { get; private set; }

    public void ConfigureDefinition(
        int catalogIndex,
        int typeCode,
        ShipStatBlock baseStats,
        int baseCost,
        int baseCarrierLoading,
        int baseLandingPodLoading,
        int baseShipLoading
    )
    {
        if (catalogIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(catalogIndex));
        }
        if (typeCode < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(typeCode));
        }
        if (baseCost < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(baseCost));
        }

        CatalogIndex = catalogIndex;
        TypeCode = typeCode;
        BaseStats = baseStats ?? ShipStatBlock.Empty;
        BaseCost = baseCost;
        BaseCarrierLoading = Math.Max(0, baseCarrierLoading);
        BaseLandingPodLoading = Math.Max(0, baseLandingPodLoading);
        BaseShipLoading = Math.Max(0, baseShipLoading);
    }
}
