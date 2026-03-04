namespace Spacer.Infrastructure.Services;

using System.Collections.Generic;
using Spacer.Application.Ports;

public sealed class InMemoryEventStateStore : IEventStateStore
{
    private readonly Dictionary<string, bool> _flags = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, int> _cooldowns = new(StringComparer.OrdinalIgnoreCase);

    public bool GetFlag(string flag)
    {
        return _flags.TryGetValue(flag, out var value) && value;
    }

    public void SetFlag(string flag, bool value)
    {
        _flags[flag] = value;
    }

    public int GetCooldownRemaining(string eventId)
    {
        return _cooldowns.TryGetValue(eventId, out var value) ? value : 0;
    }

    public void SetCooldown(string eventId, int turns)
    {
        _cooldowns[eventId] = turns < 0 ? 0 : turns;
    }
}
