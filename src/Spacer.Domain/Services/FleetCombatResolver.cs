namespace Spacer.Domain.Services;

using Spacer.Domain.ValueObjects;

public sealed class FleetCombatResolver
{
    public FleetCombatResult Resolve(FleetCombatantInputs attacker, FleetCombatantInputs defender, CombatContext context)
    {
        var attackerCombatant = FleetCombatantFactory.Create(attacker, context);
        var defenderCombatant = FleetCombatantFactory.Create(defender, context);

        return Resolve(attackerCombatant, defenderCombatant, context);
    }

    public FleetCombatResult Resolve(FleetCombatant attacker, FleetCombatant defender, CombatContext context)
    {
        var defense = CalculateDefense(defender, attackerUsesGuardBonus: attacker.UsesGuardBonus);
        var attack = attacker.AttackPower;

        if (attacker.IsConfused)
        {
            attack = attack / 2 + 1;
        }

        if (defender.IsConfused)
        {
            defense = defense / 2 + 1;
        }

        if (context.EasyMode && attacker.OverlordId == context.PlayerOverlordId)
        {
            attack = attack * 3 / 2;
        }

        var damage = defense == 0 ? attack : attack / defense;
        if (damage < context.FleetRules.MinDamage)
        {
            damage = context.FleetRules.MinDamage;
        }

        if (attacker.HasDamageMultiplier2)
        {
            damage *= 2;
        }
        if (attacker.HasDamageMultiplier3)
        {
            damage *= 3;
        }

        if (context.MaxDamagePerAttack > 0)
        {
            var cap = context.MaxDamagePerAttack * 3;
            if (damage > cap)
            {
                damage = cap;
            }
        }

        return new FleetCombatResult(attacker.FleetId, defender.FleetId, damage);
    }

    private static int CalculateDefense(FleetCombatant defender, bool attackerUsesGuardBonus)
    {
        if (defender.FleetCount <= 0)
        {
            return 1;
        }

        var baseDefense = defender.Defense / defender.FleetCount * 18 + defender.CommanderDefenseAverage * 3;
        if (defender.HasDefenseUpgrade)
        {
            baseDefense = defender.Defense / defender.FleetCount * 20 + defender.CommanderDefenseAverage * 5;
        }

        if (attackerUsesGuardBonus && defender.GuardBonus > 0)
        {
            baseDefense += defender.GuardBonus / 1000;
        }

        return baseDefense;
    }
}

public readonly record struct FleetCombatant(
    EntityId FleetId,
    EntityId OverlordId,
    int FleetCount,
    int AttackPower,
    int Defense,
    int CommanderDefenseAverage,
    bool IsConfused,
    bool HasDefenseUpgrade,
    int GuardBonus,
    bool UsesGuardBonus,
    bool HasDamageMultiplier2,
    bool HasDamageMultiplier3
);

public readonly record struct FleetCombatResult( EntityId AttackerFleetId, EntityId DefenderFleetId, int DamageUnits );
