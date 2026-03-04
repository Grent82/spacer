namespace Spacer.Application.Ports;

using System.Collections.Generic;
using Spacer.Application.Events;

public interface IEventQueue
{
    void Enqueue(EventRequest request);
    IReadOnlyList<EventRequest> Drain();
}
