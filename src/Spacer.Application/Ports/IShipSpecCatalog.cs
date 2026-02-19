namespace Spacer.Application.Ports;

using Spacer.Domain.Entities;

public interface IShipSpecCatalog
{
    IReadOnlyList<ShipSpec> GetAll();
}
