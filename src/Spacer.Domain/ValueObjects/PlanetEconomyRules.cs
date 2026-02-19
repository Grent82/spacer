namespace Spacer.Domain.ValueObjects;

public readonly record struct PlanetEconomyRules(
    int MinPopulation,
    int MaxPopulation,
    int MinLoyalty,
    int MaxLoyalty,
    int MinPublicOpinion,
    int MaxPublicOpinion,
    int MaxFunds
)
{
    public static readonly PlanetEconomyRules Default = new(
        MinPopulation: 1000,
        MaxPopulation: 1000000,
        MinLoyalty: 0,
        MaxLoyalty: 99,
        MinPublicOpinion: 0,
        MaxPublicOpinion: 99,
        MaxFunds: 9_999_999
    );
}
