namespace Spacer.Domain.ValueObjects;

using Spacer.Domain.Enums;

public readonly record struct GameRules(
    bool EasyMode,
    EntityId PlayerOverlordId,
    int CurrentMonth,
    int MonthsInYear,
    int SalaryPayoutMonth
)
{
    public static readonly GameRules Default = new(
        EasyMode: false,
        PlayerOverlordId: EntityId.None,
        CurrentMonth: 1,
        MonthsInYear: 12,
        SalaryPayoutMonth: 4
    );

    public bool IsSalaryMonth => CurrentMonth == SalaryPayoutMonth;

    public Season CurrentSeason => SeasonForMonth(CurrentMonth, MonthsInYear);

    private static Season SeasonForMonth(int month, int monthsInYear)
    {
        if (monthsInYear <= 0)
        {
            return Season.Winter;
        }

        var normalizedMonth = month <= 0 ? 1 : month;
        var seasonIndex = (normalizedMonth - 1) * 4 / monthsInYear;
        if (seasonIndex < 0)
        {
            seasonIndex = 0;
        }
        if (seasonIndex > 3)
        {
            seasonIndex = 3;
        }

        return (Season)seasonIndex;
    }
}
