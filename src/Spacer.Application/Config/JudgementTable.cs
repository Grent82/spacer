namespace Spacer.Application.Config;

using Spacer.Domain.Enums;

public sealed class JudgementTable
{
    private readonly Dictionary<JudgementKey, int> _values;

    public JudgementTable(Dictionary<JudgementKey, int> values, int defaultValue)
    {
        _values = values ?? new Dictionary<JudgementKey, int>();
        DefaultValue = defaultValue;
    }

    public static JudgementTable Empty { get; } = new(new Dictionary<JudgementKey, int>(), 0);

    public int DefaultValue { get; }

    public int GetValue(int category, PersonalityType personality)
    {
        return _values.TryGetValue(new JudgementKey(category, personality), out var value)
            ? value
            : DefaultValue;
    }

    public void SetValue(int category, PersonalityType personality, int value)
    {
        _values[new JudgementKey(category, personality)] = value;
    }
}

public readonly record struct JudgementKey(int Category, PersonalityType Personality);
