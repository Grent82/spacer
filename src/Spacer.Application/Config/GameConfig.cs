namespace Spacer.Application.Config;

using Spacer.Domain.ValueObjects;

public sealed record GameConfig(
    GameRules GameRules,
    PlanetEconomyRules PlanetEconomyRules,
    PlanetResearchRules PlanetResearchRules,
    int WeaponReleaseChanges,
    JudgementTable JudgementTable
)
{
    public static readonly GameConfig Default = new(
        GameRules.Default,
        PlanetEconomyRules.Default,
        PlanetResearchRules.Default,
        WeaponReleaseChanges: 4,
        JudgementTable: JudgementTable.Empty
    );
}
