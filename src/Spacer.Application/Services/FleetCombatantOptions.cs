namespace Spacer.Application.Services;

public readonly record struct FleetCombatantOptions(
    int? FleetCountOverride,
    int? PrimaryAttackPowerOverride,
    int? DefenseOverride,
    bool IsConfused,
    bool HasDefenseUpgrade,
    int GuardBonus,
    bool UsesGuardBonus,
    bool HasDamageMultiplier2,
    bool HasDamageMultiplier3
)
{
    public static readonly FleetCombatantOptions Default = new(
        FleetCountOverride: null,
        PrimaryAttackPowerOverride: null,
        DefenseOverride: null,
        IsConfused: false,
        HasDefenseUpgrade: false,
        GuardBonus: 0,
        UsesGuardBonus: false,
        HasDamageMultiplier2: false,
        HasDamageMultiplier3: false
    );
}
