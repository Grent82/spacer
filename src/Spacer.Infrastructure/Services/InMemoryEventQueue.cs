namespace Spacer.Infrastructure.Services;

using System;
using System.Collections.Generic;
using Spacer.Application.Events;
using Spacer.Application.Ports;

public sealed class InMemoryEventQueue : IEventQueue
{
    private readonly Queue<EventRequest> _queue = new();

    public void Enqueue(EventRequest request)
    {
        _queue.Enqueue(request);
    }

    public IReadOnlyList<EventRequest> Drain()
    {
        if (_queue.Count == 0)
        {
            return Array.Empty<EventRequest>();
        }

        var drained = new List<EventRequest>(_queue.Count);
        while (_queue.Count > 0)
        {
            drained.Add(_queue.Dequeue());
        }

        return drained;
    }
}
