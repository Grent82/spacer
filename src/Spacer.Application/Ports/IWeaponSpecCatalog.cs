namespace Spacer.Application.Ports;

using Spacer.Domain.Entities;

public interface IWeaponSpecCatalog
{
    IReadOnlyList<WeaponSpec> GetAll();
}
