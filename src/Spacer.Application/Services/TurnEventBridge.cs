namespace Spacer.Application.Services;

using System.Collections.Generic;
using Spacer.Application.Events;
using Spacer.Application.Ports;

public sealed class TurnEventBridge
{
    private readonly IEventQueue _eventQueue;

    public TurnEventBridge(IEventQueue eventQueue)
    {
        _eventQueue = eventQueue;
    }

    public void Enqueue(IReadOnlyList<EventRequest> requests)
    {
        if (requests is null || requests.Count == 0)
        {
            return;
        }

        foreach (var request in requests)
        {
            _eventQueue.Enqueue(request);
        }
    }
}
