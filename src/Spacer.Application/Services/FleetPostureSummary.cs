namespace Spacer.Application.Services;

public readonly record struct FleetPostureSummary(
    int TotalFleets,
    int FleetsMovingToOwn,
    int FleetsMovingToEnemy
)
{
    public static readonly FleetPostureSummary Empty = new(0, 0, 0);
}
