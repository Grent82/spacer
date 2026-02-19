namespace Spacer.Domain.ValueObjects;

using System;

public readonly record struct EntityId(int Value)
{
    public static readonly EntityId None = new(0);

    public static EntityId Create(int value)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Id must be non-negative.");
        }

        return new EntityId(value);
    }

    public bool IsNone => Value == 0;

    public override string ToString() => Value.ToString();
}
