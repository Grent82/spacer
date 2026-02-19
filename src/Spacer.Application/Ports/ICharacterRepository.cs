namespace Spacer.Application.Ports;

using Spacer.Domain.Entities;
using Spacer.Domain.ValueObjects;

public interface ICharacterRepository
{
    Character? FindById(EntityId id);
}
