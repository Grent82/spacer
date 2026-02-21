namespace Spacer.Domain.Services;

using System.Collections.Generic;
using Spacer.Domain.Entities;
using Spacer.Domain.Enums;
using Spacer.Domain.ValueObjects;

public sealed class FactionPoliticsService
{
    public void AdjustLoyalty(Character character, int delta, FactionPoliticsRules rules)
    {
        character.AdjustLoyalty(delta, rules);
    }

    public void ApplySuccession(
        Character newLeader,
        IReadOnlyList<Character> members,
        FactionPoliticsRules rules
    )
    {
        foreach (var member in members)
        {
            if (member.Id == newLeader.Id)
            {
                member.SetFaction(newLeader.Id, EntityId.None);
                continue;
            }

            member.SetFaction(newLeader.Id, newLeader.Id);

            var delta = ComputeSuccessionLoyaltyDelta(member, newLeader, rules);

            if (delta != 0)
            {
                member.AdjustLoyalty(delta, rules);
            }
        }
    }

    public bool ShouldDefect(Character character, FactionPoliticsRules rules, IRandomSource random)
    {
        if (character.FactionId == character.Id)
        {
            return false;
        }

        if (character.Loyalty >= rules.DefectionLoyaltyThreshold)
        {
            return false;
        }

        if (rules.DefectionRollOutOf <= 1)
        {
            return true;
        }

        return random.Next(rules.DefectionRollOutOf) == 0;
    }

    public bool TryJoinFaction( Character candidate, IReadOnlyList<Character> potentialLeaders, FactionPoliticsRules rules, IRandomSource random, EntityId playerFactionLeaderId )
    {
        if (!TrySelectJoinTarget(candidate, potentialLeaders, rules, random, playerFactionLeaderId, out var target))
        {
            return false;
        }

        var factionId = target.FactionId.IsNone ? target.Id : target.FactionId;
        candidate.SetFaction(factionId, target.Id);
        return true;
    }

    public bool TrySelectJoinTarget( Character candidate, IReadOnlyList<Character> potentialLeaders, FactionPoliticsRules rules, IRandomSource random, EntityId playerFactionLeaderId, out Character? target )
    {
        target = null;
        if (!IsEligibleForJoin(candidate, rules))
        {
            return false;
        }

        var shouldAttemptJoin = ShouldDefect(candidate, rules, random) ||
                                ShouldLeavePlayerDueToLowFriendship( candidate, playerFactionLeaderId, rules, random );

        if (!shouldAttemptJoin)
        {
            return false;
        }

        var pool = new List<Character>(potentialLeaders.Count);
        foreach (var leader in potentialLeaders)
        {
            if (!IsValidJoinTarget(candidate, leader))
            {
                continue;
            }

            pool.Add(leader);
        }

        if (pool.Count == 0)
        {
            return false;
        }

        var index = random.Next(pool.Count);
        target = pool[index];
        return true;
    }

    private static bool ShouldLeavePlayerDueToLowFriendship(
        Character candidate,
        EntityId playerFactionLeaderId,
        FactionPoliticsRules rules,
        IRandomSource random
    )
    {
        if (playerFactionLeaderId.IsNone)
        {
            return false;
        }

        if (candidate.FactionId != playerFactionLeaderId)
        {
            return false;
        }

        if (candidate.FriendshipIntimacy > rules.FriendshipLeaveThreshold)
        {
            return false;
        }

        if (rules.FriendshipLeaveRollOutOf <= 1)
        {
            return true;
        }

        if (rules.FriendshipLeaveHitCount <= 0)
        {
            return false;
        }

        if (rules.FriendshipLeaveHitCount >= rules.FriendshipLeaveRollOutOf)
        {
            return true;
        }

        return random.Next(rules.FriendshipLeaveRollOutOf) < rules.FriendshipLeaveHitCount;
    }

    private static bool IsEligibleForJoin(Character candidate, FactionPoliticsRules rules)
    {
        if (candidate.State != CharacterState.Active)
        {
            return false;
        }

        if (candidate.Age < rules.SuccessionMinAge)
        {
            return false;
        }

        if (candidate.FactionId == candidate.Id)
        {
            return false;
        }

        return true;
    }

    private static bool IsValidJoinTarget(Character candidate, Character leader)
    {
        if (leader.State != CharacterState.Active)
        {
            return false;
        }

        if (leader.Id == candidate.Id)
        {
            return false;
        }

        if (leader.FactionId != leader.Id)
        {
            return false;
        }

        if (!candidate.FactionId.IsNone && leader.FactionId == candidate.FactionId)
        {
            return false;
        }

        if (!candidate.OverlordId.IsNone && leader.Id == candidate.OverlordId)
        {
            return false;
        }

        return true;
    }

    public Character? SelectSuccessor(IReadOnlyList<Character> candidates, FactionPoliticsRules rules)
    {
        Character? best = null;
        var bestMerits = int.MinValue;
        var bestLoyalty = int.MinValue;
        var bestAge = int.MinValue;

        foreach (var candidate in candidates)
        {
            if (candidate.Age < rules.SuccessionMinAge)
            {
                continue;
            }

            if (candidate.Merits > bestMerits)
            {
                best = candidate;
                bestMerits = candidate.Merits;
                bestLoyalty = candidate.Loyalty;
                bestAge = candidate.Age;
                continue;
            }

            if (candidate.Merits == bestMerits)
            {
                if (candidate.Loyalty > bestLoyalty)
                {
                    best = candidate;
                    bestLoyalty = candidate.Loyalty;
                    bestAge = candidate.Age;
                    continue;
                }

                if (candidate.Loyalty == bestLoyalty && candidate.Age > bestAge)
                {
                    best = candidate;
                    bestAge = candidate.Age;
                }
            }
        }

        return best;
    }

    private static bool IsFamily(Character member, Character leader)
    {
        return member.FatherId == leader.Id
               || member.MotherId == leader.Id
               || member.PartnerId == leader.Id
               || leader.FatherId == member.Id
               || leader.MotherId == member.Id
               || leader.PartnerId == member.Id;
    }

    private static int ComputeSuccessionLoyaltyDelta(Character member, Character leader, FactionPoliticsRules rules)
    {
        var delta = -rules.SuccessionLoyaltyPenalty;
        if (IsFamily(member, leader))
        {
            delta += rules.SuccessionFamilyBonus;
        }

        if (leader.Diplomacy < rules.SuccessionDiplomacyThreshold)
        {
            delta -= rules.SuccessionDiplomacyPenalty;
        }
        else if (leader.Diplomacy > rules.SuccessionDiplomacyThreshold)
        {
            delta += rules.SuccessionDiplomacyBonus;
        }

        var rank = leader.Rank > 0 ? leader.Rank : leader.Merits;
        if (rank < rules.SuccessionRankThreshold1)
        {
            delta -= rules.SuccessionRankPenalty1;
        }
        if (rank < rules.SuccessionRankThreshold2)
        {
            delta -= rules.SuccessionRankPenalty2;
        }
        if (rank < rules.SuccessionRankThreshold3)
        {
            delta -= rules.SuccessionRankPenalty3;
        }
        if (rank < rules.SuccessionRankThreshold4)
        {
            delta -= rules.SuccessionRankPenalty4;
        }

        return delta;
    }
}
