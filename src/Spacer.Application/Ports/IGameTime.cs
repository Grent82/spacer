namespace Spacer.Application.Ports;

public interface IGameTime
{
    int Year { get; }
    int Month { get; }
    int MonthsInYear { get; }
    void AdvanceTurn();
    void Set(int year, int month);
}
