namespace Spacer.Domain.ValueObjects;

using System;

public sealed class ShipStatBlock
{
    public const int StatCount = 10;

    private readonly int[] _values;

    public ShipStatBlock(int[] values)
    {
        if (values is null)
        {
            throw new ArgumentNullException(nameof(values));
        }
        if (values.Length != StatCount)
        {
            throw new ArgumentException($"ShipStatBlock must have {StatCount} values.", nameof(values));
        }

        _values = (int[])values.Clone();
    }

    public static ShipStatBlock Empty { get; } = new(new int[StatCount]);

    public int Get(int index) => _values[index];

    public int[] ToArray() => (int[])_values.Clone();
}
