namespace Spacer.Domain.Services;

using Spacer.Domain.ValueObjects;

public sealed class CombatContext
{
    public CombatContext(IRandomSource random, EntityId playerOverlordId, bool easyMode)
    {
        Random = random;
        PlayerOverlordId = playerOverlordId;
        EasyMode = easyMode;
    }

    public IRandomSource Random { get; }
    public EntityId PlayerOverlordId { get; }
    public bool EasyMode { get; }

    public int MaxDamagePerAttack { get; init; } = 0;

    public DuelRules DuelRules { get; init; } = DuelRules.Default;
    public FleetCombatRules FleetRules { get; init; } = FleetCombatRules.Default;
}

public readonly record struct DuelRules( int StartingHitPoints, int DamagePerHit, int MinBattle, int MinDifference, int MaxRounds )
{
    public static readonly DuelRules Default = new( StartingHitPoints: 100, DamagePerHit: 30, MinBattle: 30, MinDifference: 3, MaxRounds: 500 );
}

public readonly record struct FleetCombatRules(int MinDamage)
{
    public static readonly FleetCombatRules Default = new(MinDamage: 9);
}
