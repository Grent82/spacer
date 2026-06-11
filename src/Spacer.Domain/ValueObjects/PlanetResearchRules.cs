namespace Spacer.Domain.ValueObjects;

public readonly record struct ResearchProgressionRules(
    int ProductionToResearchEfficiency,
    int ResearchGrowthBonusPercent,
    int ResearchDecayPercent,
    int SystemTechGrowthThreshold,
    int SystemTechMaxBonusPercent
)
{
    public static readonly ResearchProgressionRules Default = new(
        ProductionToResearchEfficiency: 10,
        ResearchGrowthBonusPercent: 25,
        ResearchDecayPercent: 2,
        SystemTechGrowthThreshold: 5000,
        SystemTechMaxBonusPercent: 50
    );
}

public readonly record struct PlanetResearchRules(
    int MinResearchLevel,
    int MaxResearchLevel,
    int MinWeaponReleaseRemaining,
    int MaxWeaponReleaseRemaining,
    int MinSystemTechLevel,
    int MaxSystemTechLevel,
    ResearchProgressionRules Progression
)
{
    public static readonly PlanetResearchRules Default = new(
        MinResearchLevel: 0,
        MaxResearchLevel: 100_000,
        MinWeaponReleaseRemaining: 0,
        MaxWeaponReleaseRemaining: 999,
        MinSystemTechLevel: 0,
        MaxSystemTechLevel: 10,
        Progression: ResearchProgressionRules.Default
    );
}
