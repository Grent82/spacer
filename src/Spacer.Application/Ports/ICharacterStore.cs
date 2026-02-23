namespace Spacer.Application.Ports;

using Spacer.Domain.Entities;

public interface ICharacterStore : ICharacterRepository, ICharacterRoster
{
    bool TryAdd(Character character);
}
