namespace Spacer.Domain.ValueObjects;

public readonly record struct Position(int X, int Y)
{
    public static readonly Position Zero = new(0, 0);
}
