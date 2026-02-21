namespace Spacer.Domain.Entities;

using System;
using Spacer.Domain.Enums;
using Spacer.Domain.ValueObjects;

public sealed class Character
{
    private Character(EntityId id, string name, Sex sex)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name must be provided.", nameof(name));
        }

        Id = id;
        Name = name;
        Sex = sex;
    }

    public static Character Create(EntityId id, string name, Sex sex) => new(id, name, sex);
    public static Character Create(int id, string name, Sex sex) => new(EntityId.Create(id), name, sex);

    public EntityId Id { get; }
    public string Name { get; private set; }

    public Sex Sex { get; private set; }
    public CharacterState State { get; private set; }

    public int Age { get; private set; }
    public int Loyalty { get; private set; }
    public int Merits { get; private set; }
    public int Rank { get; private set; }

    public EntityId FactionId { get; private set; } = EntityId.None;
    public EntityId OverlordId { get; private set; } = EntityId.None;

    public EntityId FatherId { get; private set; } = EntityId.None;
    public EntityId MotherId { get; private set; } = EntityId.None;
    public EntityId OriginFatherId { get; private set; } = EntityId.None;
    public EntityId OriginMotherId { get; private set; } = EntityId.None;
    public EntityId PartnerId { get; private set; } = EntityId.None;

    public PersonalityType Personality { get; private set; } = PersonalityType.Unknown;
    public int Cleverness { get; private set; }

    public StatBlock BaseStats { get; private set; } = StatBlock.Empty;
    public StatBlock CurrentStats { get; private set; } = StatBlock.Empty;

    public int Intelligence { get; private set; }
    public int Battle { get; private set; }
    public int Diplomacy { get; private set; }
    public int HiddenSkill { get; private set; }
    public int FriendshipIntimacy { get; private set; }

    public int PregnancyMonths { get; private set; }
    public int Infertility { get; private set; }
    public int AgeOfDeath { get; private set; }
    public int MonthOfBirth { get; private set; }
    public int SpecialFlags { get; private set; }

    public int Wounded { get; private set; }
    public int ResurrectionCount { get; private set; }
    public int Weakness { get; private set; }

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name must be provided.", nameof(name));
        }

        Name = name;
    }

    public void SetFaction(EntityId factionId, EntityId overlordId)
    {
        FactionId = factionId;
        OverlordId = overlordId;
    }

    public void SetFamily(EntityId fatherId, EntityId motherId, EntityId partnerId)
    {
        FatherId = fatherId;
        MotherId = motherId;
        PartnerId = partnerId;
    }

    public void SetPersonality(PersonalityType personality)
    {
        Personality = personality;
    }

    public void SetState(CharacterState state)
    {
        State = state;
    }

    public void SetAge(int age)
    {
        Age = age < 0 ? 0 : age;
    }

    public void SetMerits(int merits)
    {
        Merits = merits < 0 ? 0 : merits;
    }

    public void SetRank(int rank)
    {
        Rank = rank < 0 ? 0 : rank;
    }

    public void SetLoyalty(int value, FactionPoliticsRules rules)
    {
        Loyalty = Clamp(value, rules.MinLoyalty, rules.MaxLoyalty);
    }

    public void AdjustLoyalty(int delta, FactionPoliticsRules rules)
    {
        SetLoyalty(Loyalty + delta, rules);
    }

    public void SetFriendshipIntimacy(int value)
    {
        FriendshipIntimacy = Clamp(value, 0, 100);
    }

    public void AdjustFriendshipIntimacy(int delta)
    {
        SetFriendshipIntimacy(FriendshipIntimacy + delta);
    }

    public void ConfigureStats(
        int attack,
        int defense,
        int intelligence,
        int strength,
        int charisma,
        int cleverness,
        int battle,
        int diplomacy
    )
    {
        attack = Math.Max(0, attack);
        defense = Math.Max(0, defense);
        intelligence = Math.Max(0, intelligence);
        strength = Math.Max(0, strength);
        charisma = Math.Max(0, charisma);
        cleverness = Math.Max(0, cleverness);
        battle = Math.Max(0, battle);
        diplomacy = Math.Max(0, diplomacy);

        BaseStats = new StatBlock(attack, defense, intelligence, strength, charisma, cleverness);
        CurrentStats = BaseStats;

        Intelligence = intelligence;
        Cleverness = cleverness;
        Battle = battle;
        Diplomacy = diplomacy;
    }

    private static int Clamp(int value, int min, int max)
    {
        if (value < min)
        {
            return min;
        }
        if (value > max)
        {
            return max;
        }
        return value;
    }
}
