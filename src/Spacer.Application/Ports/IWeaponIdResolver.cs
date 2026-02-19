namespace Spacer.Application.Ports;

public interface IWeaponIdResolver
{
    int ResolveWeaponId(int systemId, int weaponReleaseStage, int shipTypeCode);
}
