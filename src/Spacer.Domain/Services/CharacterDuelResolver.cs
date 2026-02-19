namespace Spacer.Domain.Services;

using Spacer.Domain.Entities;
using Spacer.Domain.ValueObjects;

public sealed class CharacterDuelResolver
{
    public CharacterDuelResult Resolve(Character left, Character right, CombatContext context, bool killOnDefeat)
    {
        var rules = context.DuelRules;
        var leftHp = rules.StartingHitPoints;
        var rightHp = rules.StartingHitPoints;
        var rounds = 0;

        while (leftHp > 0 && rightHp > 0)
        {
            if (rules.MaxRounds > 0 && rounds++ >= rules.MaxRounds)
            {
                return new CharacterDuelResult(
                    WinnerId: EntityId.None,
                    LoserId: EntityId.None,
                    WinnerRemainingHp: leftHp,
                    LoserRemainingHp: rightHp,
                    IsDraw: true,
                    KillOnDefeat: killOnDefeat
                );
            }

            var leftBattle = Math.Max(rules.MinBattle, left.Battle);
            var rightBattle = Math.Max(rules.MinBattle, right.Battle);

            leftBattle /= 2;
            rightBattle /= 2;

            var leftRoll = context.Random.Next(leftBattle) + leftBattle;
            var rightRoll = context.Random.Next(rightBattle) + rightBattle;

            if (leftRoll - rightRoll > rules.MinDifference)
            {
                rightHp = Math.Max(0, rightHp - rules.DamagePerHit);
            }

            if (rightRoll - leftRoll > rules.MinDifference)
            {
                leftHp = Math.Max(0, leftHp - rules.DamagePerHit);
            }
        }

        var leftWon = leftHp > 0 && rightHp == 0;
        var winnerId = leftWon ? left.Id : right.Id;
        var loserId = leftWon ? right.Id : left.Id;
        var winnerHp = leftWon ? leftHp : rightHp;
        var loserHp = leftWon ? rightHp : leftHp;

        return new CharacterDuelResult(
            WinnerId: winnerId,
            LoserId: loserId,
            WinnerRemainingHp: winnerHp,
            LoserRemainingHp: loserHp,
            IsDraw: false,
            KillOnDefeat: killOnDefeat
        );
    }
}

public readonly record struct CharacterDuelResult(
    EntityId WinnerId,
    EntityId LoserId,
    int WinnerRemainingHp,
    int LoserRemainingHp,
    bool IsDraw,
    bool KillOnDefeat
);
