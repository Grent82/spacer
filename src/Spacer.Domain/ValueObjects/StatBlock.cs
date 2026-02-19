namespace Spacer.Domain.ValueObjects;

public readonly record struct StatBlock(int Attack, int Defense, int Intelligence, int Strength, int Charisma, int Cleverness)
{
    public static readonly StatBlock Empty = new(0, 0, 0, 0, 0, 0);
}
