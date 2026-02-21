namespace Spacer.Domain.ValueObjects;

public readonly record struct FactionPoliticsRules(
    int MinLoyalty,
    int MaxLoyalty,
    int DefectionLoyaltyThreshold,
    int DefectionRollOutOf,
    int SuccessionMinAge,
    int FriendshipLeaveThreshold,
    int FriendshipLeaveRollOutOf,
    int FriendshipLeaveHitCount,
    int SuccessionLoyaltyPenalty,
    int SuccessionFamilyBonus,
    int SuccessionPlanetLoyaltyPenalty,
    bool DefectionCreatesIndependentFaction,
    int SuccessionDiplomacyThreshold,
    int SuccessionDiplomacyPenalty,
    int SuccessionDiplomacyBonus,
    int SuccessionRankThreshold1,
    int SuccessionRankThreshold2,
    int SuccessionRankThreshold3,
    int SuccessionRankThreshold4,
    int SuccessionRankPenalty1,
    int SuccessionRankPenalty2,
    int SuccessionRankPenalty3,
    int SuccessionRankPenalty4
)
{
    public static readonly FactionPoliticsRules Default = new(
        MinLoyalty: 0,
        MaxLoyalty: 99,
        DefectionLoyaltyThreshold: 42,
        DefectionRollOutOf: 4,
        SuccessionMinAge: 15,
        FriendshipLeaveThreshold: 25,
        FriendshipLeaveRollOutOf: 6,
        FriendshipLeaveHitCount: 2,
        SuccessionLoyaltyPenalty: 15,
        SuccessionFamilyBonus: 10,
        SuccessionPlanetLoyaltyPenalty: 10,
        DefectionCreatesIndependentFaction: true,
        SuccessionDiplomacyThreshold: 50,
        SuccessionDiplomacyPenalty: 5,
        SuccessionDiplomacyBonus: 5,
        SuccessionRankThreshold1: 10_000,
        SuccessionRankThreshold2: 8_000,
        SuccessionRankThreshold3: 6_000,
        SuccessionRankThreshold4: 3_000,
        SuccessionRankPenalty1: 10,
        SuccessionRankPenalty2: 11,
        SuccessionRankPenalty3: 12,
        SuccessionRankPenalty4: 13
    );
}
