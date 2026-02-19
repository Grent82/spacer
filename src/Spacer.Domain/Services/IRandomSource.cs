namespace Spacer.Domain.Services;

public interface IRandomSource
{
    int Next(int maxExclusive);
}

public sealed class SystemRandomSource : IRandomSource
{
    private readonly Random _random;

    public SystemRandomSource()
        : this(new Random())
    {
    }

    public SystemRandomSource(Random random)
    {
        _random = random;
    }

    public int Next(int maxExclusive) => _random.Next(maxExclusive);
}
