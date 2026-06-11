namespace Spacer.Application.Services;

using System;
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
    private readonly INamePool _namePool;
    private readonly IEventQueue? _eventQueue;

    public CharacterLifecycleTurnService(
        ICharacterStore characters,
        IRandomSource random,
        IEventQueue? eventQueue = null
    )
        : this(characters, random, new EmptyNamePool(), eventQueue)
    {
    }

    public CharacterLifecycleTurnService(
        ICharacterStore characters,
        IRandomSource random,
        INamePool namePool,
        IEventQueue? eventQueue = null
    )
    {
        _characters = characters;
        _random = random;
        _namePool = namePool;
        _eventQueue = eventQueue;
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
                var pregnancyResult = TryResolvePregnancy(
                    character,
                    roster,
                    gameRules,
                    rules,
                    politicsRules,
                    ref nextGeneratedId
                );
                if (pregnancyResult.Newborn is not null)
                {
                    births.Add(pregnancyResult.Newborn);
                }
                if (pregnancyResult.Miscarriage && _eventQueue is not null)
                {
                    EnqueueMiscarriageEvent(character, gameRules);
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

        if (character.SpecialFlags == 2 && character.Age < rules.Death.ImmortalUntilAge)
        {
            return false;
        }

        if (character.AgeOfDeath > 0 && character.Age >= character.AgeOfDeath)
        {
            ApplyDeath(character, deaths, rules.Death.EnableDeathEvents);
            return true;
        }

        if (!rules.Death.EnableOldAgeDeath || character.Age <= rules.Death.OldAgeStart)
        {
            return false;
        }

        if (character.AgeOfDeath > 0)
        {
            return false;
        }

        if (rules.Death.OldAgeRollMax <= 0)
        {
            return false;
        }

        var profile = SelectOldAgeProfile(character, rules);
        var roll = _random.Next(rules.Death.OldAgeRollMax);
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
            return noble ? rules.Death.NobleFemale : rules.Death.CommonFemale;
        }

        return noble ? rules.Death.NobleMale : rules.Death.CommonMale;
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

    /// <summary>
    /// Returns pregnancy result with newborn or miscarriage flag.
    /// </summary>

    private (Character? Newborn, bool Miscarriage) TryResolvePregnancy(
        Character mother,
        IReadOnlyList<Character> roster,
        GameRules gameRules,
        CharacterLifecycleRules rules,
        FactionPoliticsRules politicsRules,
        ref int nextGeneratedId
    )
    {
        mother.SetPregnancyMonths(mother.PregnancyMonths + 1);

        // Check for loyalty-based abort.
        if (rules.Pregnancy.AbortMonth > 0
            && mother.PregnancyMonths == rules.Pregnancy.AbortMonth
            && mother.PregnancyPartnerId == gameRules.PlayerOverlordId
            && mother.Loyalty < rules.Pregnancy.AbortLoyaltyThreshold)
        {
            mother.ClearPregnancy();
            return (null, false);
        }

        // Check for miscarriage (after abort month, before full term).
        if (rules.Pregnancy.MiscarriageChancePercent > 0
            && mother.PregnancyMonths > rules.Pregnancy.MiscarriageAfterMonth
            && mother.PregnancyMonths < rules.Pregnancy.LengthMonths)
        {
            var roll = _random.Next(100);
            if (roll < rules.Pregnancy.MiscarriageChancePercent)
            {
                mother.ClearPregnancy();
                return (null, true);
            }
        }

        // Check for pregnancy milestone events.
        if (rules.Pregnancy.EnableMilestoneEvents && _eventQueue is not null)
        {
            EnqueuePregnancyMilestoneEvent(mother, gameRules);
        }

        if (rules.Pregnancy.LengthMonths <= 0 || mother.PregnancyMonths < rules.Pregnancy.LengthMonths)
        {
            return (null, false);
        }

        var father = mother.PregnancyPartnerId.IsNone ? null : _characters.FindById(mother.PregnancyPartnerId);
        var preferredFactionId = ResolvePreferredFactionId(mother, father);
        var slot = TryGetUnbornSlot(roster, preferredFactionId);
        var newborn = CreateNewborn(
            mother,
            father,
            preferredFactionId,
            gameRules,
            rules,
            politicsRules,
            nextGeneratedId,
            slot,
            out var usedSlot
        );
        if (!usedSlot)
        {
            nextGeneratedId++;
        }
        mother.ClearPregnancy();
        return (newborn, false);
    }

    private Character CreateNewborn(
        Character mother,
        Character? father,
        EntityId preferredFactionId,
        GameRules gameRules,
        CharacterLifecycleRules rules,
        FactionPoliticsRules politicsRules,
        int id,
        Character? slot,
        out bool usedSlot
    )
    {
        usedSlot = slot is not null;
        var chosenSex = slot?.Sex ?? (_random.Next(2) == 0 ? Sex.Male : Sex.Female);
        var resolvedName = ResolveNewbornName(chosenSex, preferredFactionId, mother, father, id);
        var newborn = slot ?? Character.Create(id, resolvedName, chosenSex);
        if (slot is not null && slot.Name.StartsWith("Character ", StringComparison.OrdinalIgnoreCase))
        {
            newborn.Rename(resolvedName);
        }
        newborn.SetSpecialFlags(0);
        newborn.SetInfertility(0);
        newborn.SetAgeOfDeath(0);
        newborn.SetPregnancy(EntityId.None, 0);

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
        newborn.SetFamily(fatherId, mother.Id, EntityId.None);

        if (!mother.FactionId.IsNone || !mother.OverlordId.IsNone)
        {
            newborn.SetFaction(mother.FactionId, mother.OverlordId);
        }

        var newbornState = mother.State;
        if (newbornState == CharacterState.Pirate || newbornState == CharacterState.Enslaved)
        {
            newbornState = CharacterState.Commoner;
        }

        if (fatherId == gameRules.PlayerOverlordId && mother.Rank < rules.SovereignRankThreshold && fatherId != EntityId.None)
        {
            if (father is not null && father.Rank < rules.SovereignRankThreshold)
            {
                newbornState = CharacterState.Commoner;
            }
        }

        newborn.SetState(newbornState);

        // Apply stat inheritance from parents.
        ApplyStatInheritance(newborn, mother, father, rules.StatInheritance, _random);

        return newborn;
    }

    private Character? TryGetUnbornSlot(IReadOnlyList<Character> roster, EntityId preferredFactionId)
    {
        var candidates = new List<Character>();
        foreach (var character in roster)
        {
            if (character.State != CharacterState.Unborn)
            {
                continue;
            }
            if (character.Age != 0)
            {
                continue;
            }
            if (!character.FatherId.IsNone || !character.MotherId.IsNone || !character.PartnerId.IsNone)
            {
                continue;
            }

            if (!preferredFactionId.IsNone && character.FactionId != preferredFactionId)
            {
                continue;
            }

            candidates.Add(character);
        }

        if (candidates.Count == 0)
        {
            if (preferredFactionId.IsNone)
            {
                return null;
            }

            return TryGetUnbornSlot(roster, EntityId.None);
        }

        return candidates[_random.Next(candidates.Count)];
    }

    private static EntityId ResolvePreferredFactionId(Character mother, Character? father)
    {
        if (!mother.FactionId.IsNone)
        {
            return mother.FactionId;
        }

        if (father is not null && !father.FactionId.IsNone)
        {
            return father.FactionId;
        }

        return EntityId.None;
    }

    private string ResolveNewbornName( Sex sex, EntityId preferredFactionId, Character mother, Character? father, int id )
    {
        var fromPool = _namePool.GetRandomName(sex, preferredFactionId, _random);
        if (!string.IsNullOrWhiteSpace(fromPool))
        {
            return fromPool!;
        }

        return GenerateNewbornName(mother, father, id);
    }

    private static string GenerateNewbornName(Character mother, Character? father, int id)
    {
        if (!string.IsNullOrWhiteSpace(mother.Name) && father is not null && !string.IsNullOrWhiteSpace(father.Name))
        {
            return $"Child {id} of {mother.Name} and {father.Name}";
        }

        if (!string.IsNullOrWhiteSpace(mother.Name))
        {
            return $"Child {id} of {mother.Name}";
        }

        return $"Child {id}";
    }

    private sealed class EmptyNamePool : INamePool
    {
        public string? GetRandomName(Sex sex, EntityId factionId, IRandomSource random) => null;
    }

    private void ApplyDeath(Character character, List<Character> deaths, bool enqueueEvent)
    {
        character.SetState(CharacterState.Dead);
        character.ClearPregnancy();
        deaths.Add(character);

        if (enqueueEvent && _eventQueue is not null)
        {
            EnqueueDeathEvent(character);
        }
    }

    /// <summary>
    /// Applies stat inheritance from parents to newborn using weighted averaging and randomness.
    /// </summary>
    private static void ApplyStatInheritance(
        Character newborn,
        Character mother,
        Character? father,
        NewbornStatInheritanceRules rules,
        IRandomSource random
    )
    {
        // Get parent stats (use 0 if parent is null).
        var motherStats = mother.BaseStats;
        var fatherStats = father?.BaseStats ?? new StatBlock(0, 0, 0, 0, 0, 0);

        // Calculate average from both parents.
        var avgAttack = (motherStats.Attack + fatherStats.Attack) / 2;
        var avgDefense = (motherStats.Defense + fatherStats.Defense) / 2;
        var avgIntelligence = (motherStats.Intelligence + fatherStats.Intelligence) / 2;
        var avgStrength = (motherStats.Strength + fatherStats.Strength) / 2;
        var avgCharisma = (motherStats.Charisma + fatherStats.Charisma) / 2;
        var avgCleverness = (motherStats.Cleverness + fatherStats.Cleverness) / 2;

        // Add base randomness.
        var attack = avgAttack + rules.BaseStatMin + random.Next(rules.BaseStatMax - rules.BaseStatMin);
        var defense = avgDefense + rules.BaseStatMin + random.Next(rules.BaseStatMax - rules.BaseStatMin);
        var intelligence = avgIntelligence + rules.BaseStatMin + random.Next(rules.BaseStatMax - rules.BaseStatMin);
        var strength = avgStrength + rules.BaseStatMin + random.Next(rules.BaseStatMax - rules.BaseStatMin);
        var charisma = avgCharisma + rules.BaseStatMin + random.Next(rules.BaseStatMax - rules.BaseStatMin);
        var cleverness = avgCleverness + rules.BaseStatMin + random.Next(rules.BaseStatMax - rules.BaseStatMin);

        // Add parent average bonus.
        attack += rules.StatBonusFromParentAverage;
        defense += rules.StatBonusFromParentAverage;
        intelligence += rules.StatBonusFromParentAverage;
        strength += rules.StatBonusFromParentAverage;
        charisma += rules.StatBonusFromParentAverage;
        cleverness += rules.StatBonusFromParentAverage;

        // Apply cleverness weight (slight bias toward higher cleverness parent).
        var fatherCleverness = father?.Cleverness ?? 0;
        if (mother.Cleverness > fatherCleverness)
        {
            cleverness += random.Next(rules.ClevernessWeight);
        }
        else if (father is not null)
        {
            cleverness += random.Next(rules.ClevernessWeight);
        }

        // Clamp values.
        attack = Math.Max(1, Math.Min(100, attack));
        defense = Math.Max(1, Math.Min(100, defense));
        intelligence = Math.Max(1, Math.Min(100, intelligence));
        strength = Math.Max(1, Math.Min(100, strength));
        charisma = Math.Max(1, Math.Min(100, charisma));
        cleverness = Math.Max(1, Math.Min(100, cleverness));

        // Skills based on parent averages.
        var battle = (mother.Battle + (father?.Battle ?? 0)) / 2 + random.Next(5);
        var diplomacy = (mother.Diplomacy + (father?.Diplomacy ?? 0)) / 2 + random.Next(5);

        newborn.ConfigureStats(
            attack,
            defense,
            intelligence,
            strength,
            charisma,
            cleverness,
            battle,
            diplomacy
        );
    }

    private void EnqueuePregnancyMilestoneEvent(Character mother, GameRules gameRules)
    {
        // Enqueue pregnancy milestone event (month 1, 3, 6, etc.).
        // This is a placeholder for event-driven pregnancy notifications.
        if (_eventQueue is null) return;

        var milestone = mother.PregnancyMonths;
        if (milestone == 1 || milestone == 3 || milestone == 6 || milestone == 8)
        {
            // Event would be enqueued here if event definitions exist.
            // _eventQueue.Enqueue(new EventRequest($"pregnancy_month_{milestone}", ...));
        }
    }

    private void EnqueueMiscarriageEvent(Character mother, GameRules gameRules)
    {
        // Enqueue miscarriage event for notification/handling.
        if (_eventQueue is null) return;

        // Event would be enqueued here if event definitions exist.
        // _eventQueue.Enqueue(new EventRequest("miscarriage", ...));
    }

    private void EnqueueDeathEvent(Character deceased)
    {
        // Enqueue death event for notification/handling.
        if (_eventQueue is null) return;

        // Find related characters for context (partner, children).
        var partner = !deceased.PartnerId.IsNone ? _characters.FindById(deceased.PartnerId) : null;
        var children = new List<Character>();
        foreach (var character in _characters.GetAll())
        {
            if (character.FatherId == deceased.Id || character.MotherId == deceased.Id)
            {
                children.Add(character);
            }
        }

        // Event would be enqueued here if event definitions exist.
        // _eventQueue.Enqueue(new EventRequest("character_death", ...));
    }
}
