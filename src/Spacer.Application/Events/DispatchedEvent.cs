namespace Spacer.Application.Events;

public sealed record DispatchedEvent(string EventId, EventRenderResult Result);
