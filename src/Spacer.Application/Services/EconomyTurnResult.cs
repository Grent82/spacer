namespace Spacer.Application.Services;

public readonly record struct EconomyTurnResult(
    int ProductionIncome,
    int SalaryIncome,
    int PublicOpinionDelta,
    int PopulationDelta,
    int LoyaltyDelta
);
