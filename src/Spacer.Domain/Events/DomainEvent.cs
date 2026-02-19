namespace Spacer.Domain.Events;

using System;

public abstract record DomainEvent(DateTimeOffset OccurredAt);
