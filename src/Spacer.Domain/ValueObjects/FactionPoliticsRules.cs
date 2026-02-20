namespace Spacer.Domain.ValueObjects;

public readonly record struct FactionPoliticsRules(
    int MinLoyalty,
    int MaxLoyalty,
    int DefectionLoyaltyThreshold,
    int DefectionRollOutOf,
    int SuccessionMinAge,
    int FriendshipLeaveThreshold,
    int FriendshipLeaveRollOutOf,
    int FriendshipLeaveHitCount
)
{
    public static readonly FactionPoliticsRules Default = new(
        MinLoyalty: 0,
        MaxLoyalty: 100,
        DefectionLoyaltyThreshold: 42,
        DefectionRollOutOf: 4,
        SuccessionMinAge: 15,
        FriendshipLeaveThreshold: 25,
        FriendshipLeaveRollOutOf: 6,
        FriendshipLeaveHitCount: 2
    );
}
