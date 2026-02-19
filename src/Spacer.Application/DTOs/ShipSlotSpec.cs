namespace Spacer.Application.DTOs;

using Spacer.Domain.ValueObjects;

public readonly record struct ShipSlotSpec(
    int SlotIndex,
    EntityId ShipSpecId,
    int ShipTypeCode,
    int WeaponId,
    ShipStatBlock OptimizedStats,
    int Cost,
    int CarrierLoading,
    int LandingPodLoading,
    int ShipLoading
);
