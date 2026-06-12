namespace Spacer.Domain.ValueObjects;

public readonly record struct OldAgeProfile(
    int Stage1Age,
    int Stage1RollThreshold,
    int Stage2Age,
    int Stage2RollThreshold,
    int MaxAge
);

public readonly record struct NewbornStatInheritanceRules(
    int BaseStatMin,
    int BaseStatMax,
    int StatBonusFromParentAverage,
    int ClevernessWeight,
    int PersonalityRandomnessWeight
)
{
    public static readonly NewbornStatInheritanceRules Default = new(
        BaseStatMin: 5,
        BaseStatMax: 15,
        StatBonusFromParentAverage: 2,
        ClevernessWeight: 10,
        PersonalityRandomnessWeight: 5
    );
}

public readonly record struct PregnancyRules(
    int LengthMonths,
    int AbortMonth,
    int AbortLoyaltyThreshold,
    int MiscarriageChancePercent,
    int MiscarriageAfterMonth,
    bool EnableMilestoneEvents,
    int ConceptionChancePercent,
    bool ConceptionWithPartnerOnly,
    int ConceptionInfertilityThreshold
)
{
    public static readonly PregnancyRules Default = new(
        LengthMonths: 9,
        AbortMonth: 2,
        AbortLoyaltyThreshold: 20,
        MiscarriageChancePercent: 5,
        MiscarriageAfterMonth: 2,
        EnableMilestoneEvents: false,
        ConceptionChancePercent: 15,
        ConceptionWithPartnerOnly: true,
        ConceptionInfertilityThreshold: 100
    );
}

public readonly record struct DeathRules(
    bool EnableOldAgeDeath,
    int ImmortalUntilAge,
    int OldAgeStart,
    int OldAgeRollMax,
    OldAgeProfile NobleMale,
    OldAgeProfile NobleFemale,
    OldAgeProfile CommonMale,
    OldAgeProfile CommonFemale,
    bool EnableDeathEvents,
    bool EnableDiseaseDeath,
    int DiseaseDeathChancePercent
)
{
    public static readonly DeathRules Default = new(
        EnableOldAgeDeath: true,
        ImmortalUntilAge: 72,
        OldAgeStart: 60,
        OldAgeRollMax: 20,
        NobleMale: new OldAgeProfile(Stage1Age: 60, Stage1RollThreshold: 5, Stage2Age: 70, Stage2RollThreshold: 8, MaxAge: 80),
        NobleFemale: new OldAgeProfile(Stage1Age: 65, Stage1RollThreshold: 2, Stage2Age: 75, Stage2RollThreshold: 5, MaxAge: 85),
        CommonMale: new OldAgeProfile(Stage1Age: 60, Stage1RollThreshold: 2, Stage2Age: 70, Stage2RollThreshold: 5, MaxAge: 85),
        CommonFemale: new OldAgeProfile(Stage1Age: 65, Stage1RollThreshold: 2, Stage2Age: 75, Stage2RollThreshold: 5, MaxAge: 85),
        EnableDeathEvents: true,
        EnableDiseaseDeath: false,
        DiseaseDeathChancePercent: 1
    );
}

public readonly record struct CharacterLifecycleRules(
    int NewbornAge,
    int NewbornRank,
    int GeneratedIdStart,
    int SovereignRankThreshold,
    int NobleRankThreshold,
    int TrainingAgeCap,
    NewbornStatInheritanceRules StatInheritance,
    PregnancyRules Pregnancy,
    DeathRules Death
)
{
    public static readonly CharacterLifecycleRules Default = new(
        NewbornAge: 1,
        NewbornRank: 1000,
        GeneratedIdStart: 10000,
        SovereignRankThreshold: 13000,
        NobleRankThreshold: 12100,
        TrainingAgeCap: 15,
        StatInheritance: NewbornStatInheritanceRules.Default,
        Pregnancy: PregnancyRules.Default,
        Death: DeathRules.Default
    );
}
