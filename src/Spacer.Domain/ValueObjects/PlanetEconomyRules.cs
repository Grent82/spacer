namespace Spacer.Domain.ValueObjects;

public readonly record struct PlanetEconomyRules(
    int MinPopulation,
    int MaxPopulation,
    int MinLoyalty,
    int MaxLoyalty,
    int MinPublicOpinion,
    int MaxPublicOpinion,
    int MaxFunds,
    int PopulationGrowthDivisor,
    int PublicOpinionLowThreshold,
    int PublicOpinionHighThreshold,
    int PublicOpinionDriftPerTurn,
    int FleetPostureMinFleets,
    int FleetPostureOwnPenalty,
    int FleetPostureEnemyPenalty,
    int LowLoyaltyPopulationThreshold,
    int LowLoyaltyPopulationCrashOdds,
    int LowLoyaltyPopulationCrashTrigger,
    int LowLoyaltyPopulationCrashDivisor,
    int LowLoyaltyResetValue
)
{
    public static readonly PlanetEconomyRules Default = new(
        MinPopulation: 1000,
        MaxPopulation: 1000000,
        MinLoyalty: 0,
        MaxLoyalty: 99,
        MinPublicOpinion: 0,
        MaxPublicOpinion: 99,
        MaxFunds: 9_999_999,
        PopulationGrowthDivisor: 1000,
        PublicOpinionLowThreshold: 45,
        PublicOpinionHighThreshold: 55,
        PublicOpinionDriftPerTurn: 6,
        FleetPostureMinFleets: 2,
        FleetPostureOwnPenalty: 3,
        FleetPostureEnemyPenalty: 4,
        LowLoyaltyPopulationThreshold: 30,
        LowLoyaltyPopulationCrashOdds: 5,
        LowLoyaltyPopulationCrashTrigger: 2,
        LowLoyaltyPopulationCrashDivisor: 20,
        LowLoyaltyResetValue: 5
    );
}
