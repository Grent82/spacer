namespace Spacer.Application.Ports;

using Spacer.Application.DTOs;

public interface IFactionCatalog
{
    FactionInfo? FindById(int id);
}
