namespace Spacer.Application.Ports;

using Spacer.Application.Events;

public interface IEventCatalog
{
    IReadOnlyList<EventDefinition> GetAll();
    IReadOnlyList<EventDefinition> GetByTrigger(string trigger);
    EventDefinition? FindById(string id);
}
