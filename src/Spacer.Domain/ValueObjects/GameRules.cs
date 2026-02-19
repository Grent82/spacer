namespace Spacer.Domain.ValueObjects;

public readonly record struct GameRules(
    bool EasyMode,
    EntityId PlayerOverlordId
)
{
    public static readonly GameRules Default = new(
        EasyMode: false,
        PlayerOverlordId: EntityId.None
    );
}
