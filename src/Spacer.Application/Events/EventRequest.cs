namespace Spacer.Application.Events;

public sealed record EventRequest(string EventId, EventRenderContext Context);
