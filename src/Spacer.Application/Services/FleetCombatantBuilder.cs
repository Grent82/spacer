namespace Spacer.Application.Services;

using Spacer.Domain.Entities;
using Spacer.Domain.Services;

public sealed class FleetCombatantBuilder
{
    public FleetCombatantInputs Build( Fleet fleet, IReadOnlyList<Character> commanders, FleetCombatantOptions options )
    {
        var fleetCount = options.FleetCountOverride ?? fleet.NumberOfShips;
        var primaryAttackPower = options.PrimaryAttackPowerOverride ?? fleet.Attack;
        var defense = options.DefenseOverride ?? fleet.Defense;

        return new FleetCombatantInputs(
            FleetId: fleet.Id,
            OverlordId: fleet.OverlordId,
            FleetCount: fleetCount,
            PrimaryAttackPower: primaryAttackPower,
            Defense: defense,
            Commanders: commanders,
            IsConfused: options.IsConfused,
            HasDefenseUpgrade: options.HasDefenseUpgrade,
            GuardBonus: options.GuardBonus,
            UsesGuardBonus: options.UsesGuardBonus,
            HasDamageMultiplier2: options.HasDamageMultiplier2,
            HasDamageMultiplier3: options.HasDamageMultiplier3
        );
    }
}
