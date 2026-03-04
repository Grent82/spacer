namespace Spacer.Application.Ports;

public interface IEventStateStore
{
    bool GetFlag(string flag);
    void SetFlag(string flag, bool value);

    int GetCooldownRemaining(string eventId);
    void SetCooldown(string eventId, int turns);
}
