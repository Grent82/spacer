namespace Spacer.Application.Events;

using Spacer.Application.Ports;
using Spacer.Domain.Services;
using Spacer.Domain.ValueObjects;

public sealed class EventContext
{
    public EventContext(IRandomSource random, IEventStateStore stateStore)
    {
        Random = random;
        StateStore = stateStore;
    }

    public EntityId PlayerId { get; init; } = EntityId.None;
    public EntityId ActorId { get; init; } = EntityId.None;
    public EntityId TargetId { get; init; } = EntityId.None;
    public EntityId ChildId { get; init; } = EntityId.None;
    public EntityId PlanetId { get; init; } = EntityId.None;
    public int Turn { get; init; }
    public int Month { get; init; }

    public IRandomSource Random { get; }
    public IEventStateStore StateStore { get; }
}
