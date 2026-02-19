namespace Spacer.Application.Ports;

public interface IWeaponIdResolver
{
    int ResolveWeaponId(int factionId, int tier, int shipTypeCode);
}
