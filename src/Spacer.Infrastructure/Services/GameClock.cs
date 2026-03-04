namespace Spacer.Infrastructure.Services;

using System;
using Spacer.Application.Ports;

public sealed class GameClock : IGameTime
{
    private int _year;
    private int _month;
    private readonly int _monthsInYear;

    public GameClock(int startYear, int startMonth, int monthsInYear)
    {
        _monthsInYear = Math.Max(1, monthsInYear);
        _year = startYear <= 0 ? 1 : startYear;
        _month = NormalizeMonth(startMonth, _monthsInYear);
    }

    public int Year => _year;
    public int Month => _month;
    public int MonthsInYear => _monthsInYear;

    public void AdvanceTurn()
    {
        _month++;
        if (_month > _monthsInYear)
        {
            _month = 1;
            _year++;
        }
    }

    public void Set(int year, int month)
    {
        _year = year <= 0 ? 1 : year;
        _month = NormalizeMonth(month, _monthsInYear);
    }

    private static int NormalizeMonth(int month, int monthsInYear)
    {
        if (month <= 0)
        {
            return 1;
        }

        if (month > monthsInYear)
        {
            return monthsInYear;
        }

        return month;
    }
}
