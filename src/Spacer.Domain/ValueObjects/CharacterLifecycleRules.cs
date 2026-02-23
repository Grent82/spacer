namespace Spacer.Domain.ValueObjects;

public readonly record struct OldAgeProfile(
    int Stage1Age,
    int Stage1RollThreshold,
    int Stage2Age,
    int Stage2RollThreshold,
    int MaxAge
);

public readonly record struct CharacterLifecycleRules(
    int PregnancyLengthMonths,
    int PregnancyAbortMonth,
    int PregnancyAbortLoyaltyThreshold,
    int NewbornAge,
    int NewbornRank,
    int GeneratedIdStart,
    bool EnableOldAgeDeath,
    int ImmortalUntilAge,
    int OldAgeStart,
    int OldAgeRollMax,
    int NobleRankThreshold,
    OldAgeProfile NobleMale,
    OldAgeProfile NobleFemale,
    OldAgeProfile CommonMale,
    OldAgeProfile CommonFemale,
    int TrainingAgeCap
)
{
    public static readonly CharacterLifecycleRules Default = new(
        PregnancyLengthMonths: 9,
        PregnancyAbortMonth: 2,
        PregnancyAbortLoyaltyThreshold: 20,
        NewbornAge: 1,
        NewbornRank: 1000,
        GeneratedIdStart: 10000,
        EnableOldAgeDeath: true,
        ImmortalUntilAge: 72,
        OldAgeStart: 60,
        OldAgeRollMax: 20,
        NobleRankThreshold: 12100,
        NobleMale: new OldAgeProfile(Stage1Age: 60, Stage1RollThreshold: 5, Stage2Age: 70, Stage2RollThreshold: 8, MaxAge: 80),
        NobleFemale: new OldAgeProfile(Stage1Age: 65, Stage1RollThreshold: 2, Stage2Age: 75, Stage2RollThreshold: 5, MaxAge: 85),
        CommonMale: new OldAgeProfile(Stage1Age: 60, Stage1RollThreshold: 2, Stage2Age: 70, Stage2RollThreshold: 5, MaxAge: 85),
        CommonFemale: new OldAgeProfile(Stage1Age: 65, Stage1RollThreshold: 2, Stage2Age: 75, Stage2RollThreshold: 5, MaxAge: 85),
        TrainingAgeCap: 15
    );
}
