namespace Spacer.Application.DTOs;

using Spacer.Domain.ValueObjects;

public sealed record PlanetFleetSpecSet(
    EntityId PlanetId,
    WeaponResearchCategory Category,
    IReadOnlyList<ShipSlotSpec> Slots
);
