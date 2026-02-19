namespace Spacer.Domain.ValueObjects;

using System;

public readonly record struct ActionId(int Value)
{
    public static readonly ActionId None = new(0);

    public static ActionId Create(int value)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "ActionId must be non-negative.");
        }

        return new ActionId(value);
    }

    public bool IsNone => Value == 0;

    public override string ToString() => Value.ToString();
}
