namespace Spacer.Tests;

using Spacer.Domain.Services;

/// <summary>
/// A deterministic fake random source for testing.
/// Returns values from a pre-defined sequence or a fixed value.
/// </summary>
public sealed class FakeRandomSource : IRandomSource
{
    private readonly int[] _sequence;
    private int _index;
    private readonly int? _fixedValue;

    /// <summary>
    /// Creates a fake random source that returns a fixed value every time.
    /// </summary>
    public FakeRandomSource(int fixedValue)
    {
        _fixedValue = fixedValue;
        _sequence = Array.Empty<int>();
    }

    /// <summary>
    /// Creates a fake random source that returns values from a sequence.
    /// When the sequence is exhausted, returns 0.
    /// </summary>
    public FakeRandomSource(params int[] sequence)
    {
        _sequence = sequence ?? Array.Empty<int>();
        _index = 0;
        _fixedValue = null;
    }

    public int Next(int maxExclusive)
    {
        if (_fixedValue.HasValue)
        {
            return _fixedValue.Value % Math.Max(1, maxExclusive);
        }

        if (_index < _sequence.Length)
        {
            return _sequence[_index++] % Math.Max(1, maxExclusive);
        }

        return 0;
    }

    /// <summary>
    /// Resets the sequence to start from the beginning.
    /// </summary>
    public void Reset()
    {
        _index = 0;
    }
}
