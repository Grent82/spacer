namespace Spacer.Application.Ports;

using System.Text.Json;

public interface IEventStateStore
{
    bool GetFlag(string flag);
    void SetFlag(string flag, bool value);

    int GetCooldownRemaining(string eventId);
    void SetCooldown(string eventId, int turns);

    /// <summary>
    /// Gets a variable value by key.
    /// </summary>
    JsonElement GetVar(string key);

    /// <summary>
    /// Sets a variable value by key.
    /// </summary>
    void SetVar(string key, JsonElement value);
}
