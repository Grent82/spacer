namespace Spacer.Domain.ValueObjects;

public readonly record struct PlanetResearchRules(
    int MinResearchLevel,
    int MaxResearchLevel,
    int MinWeaponReleaseRemaining,
    int MaxWeaponReleaseRemaining,
    int MinSystemTechLevel,
    int MaxSystemTechLevel
)
{
    public static readonly PlanetResearchRules Default = new(
        MinResearchLevel: 0,
        MaxResearchLevel: 100_000,
        MinWeaponReleaseRemaining: 0,
        MaxWeaponReleaseRemaining: 999,
        MinSystemTechLevel: 0,
        MaxSystemTechLevel: 10
    );
}
