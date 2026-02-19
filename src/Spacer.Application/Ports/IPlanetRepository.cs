namespace Spacer.Application.Ports;

using Spacer.Domain.Entities;
using Spacer.Domain.ValueObjects;

public interface IPlanetRepository
{
    IReadOnlyList<Planet> GetAll();
    Planet? FindById(EntityId id);
}
