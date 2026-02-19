namespace Spacer.Domain.ValueObjects;

using System;

public readonly record struct AssignmentId(int Value)
{
    public static readonly AssignmentId None = new(0);

    public static AssignmentId Create(int value)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "AssignmentId must be non-negative.");
        }

        return new AssignmentId(value);
    }

    public bool IsNone => Value == 0;

    public override string ToString() => Value.ToString();
}
