namespace Spacer.Application.Ports;

using Spacer.Domain.Entities;

public interface IItemCatalog
{
    IReadOnlyList<ItemSpec> GetAll();
}
