namespace Spacer.Domain.ValueObjects;

public readonly record struct Money(int Amount)
{
    public static readonly Money Zero = new(0);

    public static Money operator +(Money a, Money b) => new(a.Amount + b.Amount);
    public static Money operator -(Money a, Money b) => new(a.Amount - b.Amount);
}
