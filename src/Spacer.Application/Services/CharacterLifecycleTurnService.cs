namespace Spacer.Application.Services;

using System.Collections.Generic;
using Spacer.Application.Ports;
using Spacer.Domain.Entities;
using Spacer.Domain.Enums;
using Spacer.Domain.Services;
using Spacer.Domain.ValueObjects;

public sealed class CharacterLifecycleTurnService
{
    private readonly ICharacterStore _characters;
    private readonly IRandomSource _random;

    public CharacterLifecycleTurnService(ICharacterStore characters, IRandomSource random)
    {
        _characters = characters;
        _random = random;
    }

    public CharacterLifecycleResult Apply(
        GameRules gameRules,
        CharacterLifecycleRules rules,
        FactionPoliticsRules politicsRules
    )
    {
        var roster = _characters.GetAll();
        if (roster.Count == 0)
        {
            return CharacterLifecycleResult.Empty;
        }

        var births = new List<Character>();
        var deaths = new List<Character>();
        var nextGeneratedId = GetNextGeneratedId(roster, rules.GeneratedIdStart);

        foreach (var character in roster)
        {
            if (!ShouldProcess(character))
            {
                continue;
            }

            NormalizeBirthMonth(character, gameRules);

            if (ShouldAge(character, gameRules, rules))
            {
                if (ApplyBirthday(character, rules, deaths))
                {
                    continue;
                }
            }

            if (ShouldProgressPregnancy(character))
            {
                var newborn = TryResolvePregnancy(character, gameRules, rules, politicsRules, ref nextGeneratedId);
                if (newborn is not null)
                {
                    births.Add(newborn);
                }
            }
        }

        foreach (var newborn in births)
        {
            _characters.TryAdd(newborn);
        }

        return births.Count == 0 && deaths.Count == 0
            ? CharacterLifecycleResult.Empty
            : new CharacterLifecycleResult(births, deaths);
    }

    private static int GetNextGeneratedId(IReadOnlyList<Character> roster, int start)
    {
        var max = start - 1;
        foreach (var character in roster)
        {
            if (character.Id.Value > max)
            {
                max = character.Id.Value;
            }
        }

        return max + 1;
    }

    private static bool ShouldProcess(Character character)
    {
        if (character.State == CharacterState.Dead || character.State == CharacterState.Inactive)
        {
            return false;
        }

        var stateValue = (int)character.State;
        if (stateValue >= 10
            && character.State != CharacterState.Enslaved
            && character.State != CharacterState.NoPlayer
            && character.State != CharacterState.Commoner)
        {
            return false;
        }

        return character.State != CharacterState.Unknown;
    }

    private static void NormalizeBirthMonth(Character character, GameRules gameRules)
    {
        if (gameRules.MonthsInYear <= 0)
        {
            return;
        }

        var month = character.MonthOfBirth;
        if (month <= 0 || month > gameRules.MonthsInYear)
        {
            var current = gameRules.CurrentMonth <= 0 ? 1 : gameRules.CurrentMonth;
            character.SetMonthOfBirth(current);
        }
    }

    private static bool ShouldAge(Character character, GameRules gameRules, CharacterLifecycleRules rules)
    {
        if (gameRules.MonthsInYear <= 0)
        {
            return false;
        }

        if (character.MonthOfBirth != gameRules.CurrentMonth)
        {
            return false;
        }

        if (character.State == CharacterState.VillageGirl && character.Age > rules.TrainingAgeCap)
        {
            return false;
        }

        if (character.State == CharacterState.Unborn && character.Age == 0)
        {
            return false;
        }

        return true;
    }

    private bool ApplyBirthday(
        Character character,
        CharacterLifecycleRules rules,
        List<Character> deaths
    )
    {
        character.SetAge(character.Age + 1);

        if (character.SpecialFlags == 2 && character.Age < rules.ImmortalUntilAge)
        {
            return false;
        }

        if (character.AgeOfDeath > 0 && character.Age >= character.AgeOfDeath)
        {
            ApplyDeath(character, deaths);
            return true;
        }

        if (!rules.EnableOldAgeDeath || character.Age <= rules.OldAgeStart)
        {
            return false;
        }

        if (character.AgeOfDeath > 0)
        {
            return false;
        }

        if (rules.OldAgeRollMax <= 0)
        {
            return false;
        }

        var profile = SelectOldAgeProfile(character, rules);
        var roll = _random.Next(rules.OldAgeRollMax);
        var shouldSchedule = false;

        if (character.Age > profile.Stage1Age && roll < profile.Stage1RollThreshold)
        {
            shouldSchedule = true;
        }

        if (character.Age > profile.Stage2Age && roll < profile.Stage2RollThreshold)
        {
            shouldSchedule = true;
        }

        if (character.Age > profile.MaxAge)
        {
            shouldSchedule = true;
        }

        if (shouldSchedule)
        {
            character.SetAgeOfDeath(character.Age + 1);
        }

        return false;
    }

    private static OldAgeProfile SelectOldAgeProfile(Character character, CharacterLifecycleRules rules)
    {
        var noble = character.Rank >= rules.NobleRankThreshold;
        if (character.Sex == Sex.Female)
        {
            return noble ? rules.NobleFemale : rules.CommonFemale;
        }

        return noble ? rules.NobleMale : rules.CommonMale;
    }

    private static bool ShouldProgressPregnancy(Character character)
    {
        if (character.PregnancyMonths <= 0)
        {
            return false;
        }

        if (character.Sex != Sex.Female)
        {
            return false;
        }

        return character.State == CharacterState.Active
            || character.State == CharacterState.Pirate
            || character.State == CharacterState.Wanderer
            || character.State == CharacterState.VillageGirl
            || character.State == CharacterState.Enslaved
            || character.State == CharacterState.Commoner;
    }

    private Character? TryResolvePregnancy(
        Character mother,
        GameRules gameRules,
        CharacterLifecycleRules rules,
        FactionPoliticsRules politicsRules,
        ref int nextGeneratedId
    )
    {
        mother.SetPregnancyMonths(mother.PregnancyMonths + 1);

        if (rules.PregnancyAbortMonth > 0
            && mother.PregnancyMonths == rules.PregnancyAbortMonth
            && mother.PregnancyPartnerId == gameRules.PlayerOverlordId
            && mother.Loyalty < rules.PregnancyAbortLoyaltyThreshold)
        {
            mother.ClearPregnancy();
            return null;
        }

        if (rules.PregnancyLengthMonths <= 0 || mother.PregnancyMonths < rules.PregnancyLengthMonths)
        {
            return null;
        }

        var newborn = CreateNewborn(mother, gameRules, rules, politicsRules, nextGeneratedId);
        nextGeneratedId++;
        mother.ClearPregnancy();
        return newborn;
    }

    private Character CreateNewborn(
        Character mother,
        GameRules gameRules,
        CharacterLifecycleRules rules,
        FactionPoliticsRules politicsRules,
        int id
    )
    {
        var sex = _random.Next(2) == 0 ? Sex.Male : Sex.Female;
        var newborn = Character.Create(id, $"Child {id}", sex);

        newborn.SetAge(rules.NewbornAge);
        newborn.SetRank(rules.NewbornRank);
        newborn.SetLoyalty(politicsRules.MaxLoyalty, politicsRules);

        var birthMonth = gameRules.CurrentMonth - 1;
        if (birthMonth <= 0)
        {
            birthMonth = gameRules.MonthsInYear > 0 ? gameRules.MonthsInYear : 12;
        }
        newborn.SetMonthOfBirth(birthMonth);

        var fatherId = mother.PregnancyPartnerId;
        newborn.SetFamily(
            fatherId,
            mother.Id,
            EntityId.None
        );

        if (!mother.FactionId.IsNone || !mother.OverlordId.IsNone)
        {
            newborn.SetFaction(mother.FactionId, mother.OverlordId);
        }

        var newbornState = mother.State;
        if (newbornState == CharacterState.Pirate || newbornState == CharacterState.Enslaved)
        {
            newbornState = CharacterState.Commoner;
        }

        if (fatherId == gameRules.PlayerOverlordId
            && mother.Rank < rules.NobleRankThreshold
            && fatherId != EntityId.None)
        {
            var father = _characters.FindById(fatherId);
            if (father is not null && father.Rank < rules.NobleRankThreshold)
            {
                newbornState = CharacterState.Commoner;
            }
        }

        newborn.SetState(newbornState);
        return newborn;
    }

    private static void ApplyDeath(Character character, List<Character> deaths)
    {
        character.SetState(CharacterState.Dead);
        character.ClearPregnancy();
        deaths.Add(character);
    }
}
