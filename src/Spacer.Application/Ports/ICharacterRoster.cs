namespace Spacer.Application.Ports;

using Spacer.Domain.Entities;

public interface ICharacterRoster
{
    IReadOnlyList<Character> GetAll();
}
