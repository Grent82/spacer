namespace Spacer.Domain.ValueObjects;

public readonly record struct AgeRange(int Min, int Max)
{
    public bool Contains(int age) => age >= Min && age <= Max;
}
