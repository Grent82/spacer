namespace Spacer.Domain.Services;

using System;
using Spacer.Domain.Entities;
using Spacer.Domain.ValueObjects;

public static class FleetCombatantFactory
{
    public static FleetCombatant Create(FleetCombatantInputs inputs, CombatContext context)
    {
        var commanderAttackAverage = ComputeCommanderAttackAverage(inputs.Commanders);
        var commanderDefenseAverage = ComputeCommanderDefenseAverage(inputs.Commanders);

        var attackPower = ComputeTotalAttackPower(
            inputs.PrimaryAttackPower,
            commanderAttackAverage,
            context,
            inputs.OverlordId
        );

        return new FleetCombatant(
            FleetId: inputs.FleetId,
            OverlordId: inputs.OverlordId,
            FleetCount: Math.Max(0, inputs.FleetCount),
            AttackPower: Math.Max(0, attackPower),
            Defense: Math.Max(0, inputs.Defense),
            CommanderDefenseAverage: Math.Max(0, commanderDefenseAverage),
            IsConfused: inputs.IsConfused,
            HasDefenseUpgrade: inputs.HasDefenseUpgrade,
            GuardBonus: Math.Max(0, inputs.GuardBonus),
            UsesGuardBonus: inputs.UsesGuardBonus,
            HasDamageMultiplier2: inputs.HasDamageMultiplier2,
            HasDamageMultiplier3: inputs.HasDamageMultiplier3
        );
    }

    public static int ComputeCommanderAttackAverage(IReadOnlyList<Character> commanders)
    {
        return ComputeWeightedAverage(commanders, c => c.CurrentStats.Attack);
    }

    public static int ComputeCommanderDefenseAverage(IReadOnlyList<Character> commanders)
    {
        return ComputeWeightedAverage(commanders, c => c.CurrentStats.Defense);
    }

    public static int ComputeTotalAttackPower(
        int primaryAttackPower,
        int commanderAttackAverage,
        CombatContext context,
        EntityId attackerOverlordId
    )
    {
        var total = primaryAttackPower / 3 + commanderAttackAverage * primaryAttackPower / 400;

        if (context.EasyMode && attackerOverlordId == context.PlayerOverlordId)
        {
            total *= 2;
        }

        return total;
    }

    private static int ComputeWeightedAverage(IReadOnlyList<Character> commanders, Func<Character, int> selector)
    {
        if (commanders.Count == 0)
        {
            return 0;
        }

        var sum = 0;
        var weight = 0;

        for (var i = 0; i < commanders.Count; i++)
        {
            var value = Math.Max(0, selector(commanders[i]));
            var add = i == 0 ? 2 : 1;
            sum += value * add;
            weight += add;
        }

        return weight == 0 ? 0 : sum / weight;
    }
}
