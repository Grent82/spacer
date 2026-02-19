namespace Spacer.Domain.Services;

using System.Collections.Generic;
using Spacer.Domain.Entities;
using Spacer.Domain.ValueObjects;

public readonly record struct FleetCombatantInputs(
    EntityId FleetId,
    EntityId OverlordId,
    int FleetCount,
    int Defense,
    int PrimaryAttackPower,
    int GuardBonus,
    bool UsesGuardBonus,
    bool IsConfused,
    bool HasDefenseUpgrade,
    bool HasDamageMultiplier2,
    bool HasDamageMultiplier3,
    IReadOnlyList<Character> Commanders
);
