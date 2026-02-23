namespace Spacer.Application.Ports;

using Spacer.Domain.Enums;
using Spacer.Domain.Services;
using Spacer.Domain.ValueObjects;

public interface INamePool
{
    string? GetRandomName(Sex sex, EntityId factionId, IRandomSource random);
}
