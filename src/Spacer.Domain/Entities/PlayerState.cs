namespace Spacer.Domain.Entities;

using Spacer.Domain.ValueObjects;

public sealed class PlayerState
{
    private PlayerState(EntityId playerCharacterId)
    {
        PlayerCharacterId = playerCharacterId;
    }

    public static PlayerState Create(EntityId playerCharacterId) => new(playerCharacterId);
    public static PlayerState Create(int playerCharacterId) => new(EntityId.Create(playerCharacterId));

    public EntityId PlayerCharacterId { get; private set; } = EntityId.None;
    public ActionId CurrentActionId { get; private set; } = ActionId.None;
    public AssignmentId AssignmentId { get; private set; } = AssignmentId.None;

    public bool EasyMode { get; private set; }
    public string TrialFlag { get; private set; } = string.Empty;

    public int DaysElapsed { get; private set; }
    public int GlobalIdCounter { get; private set; }

    public Money Wealth { get; private set; } = Money.Zero;

    public EntityId PlayerShipId { get; private set; } = EntityId.None;
    public int PlayerLevel { get; private set; }
    public int PlayerXp { get; private set; }
}
