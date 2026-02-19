namespace Spacer.Application.Ports;

public interface IWeaponNameFormatter
{
    string BuildName(int factionId, int shipTypeCode, int tier);
    string BuildImageKey(int factionId, int shipTypeCode, int tier);
}
