namespace Spacer.Application.Events;

using System;
using System.Collections.Generic;

public sealed record EventExecutionResult(
    bool Completed,
    IReadOnlyList<EventLogEntry> Log,
    EventChoice? PendingChoice
)
{
    public static readonly EventExecutionResult Empty =
        new(true, Array.Empty<EventLogEntry>(), null);
}

public sealed record EventLogEntry(string Type, string Text);

public sealed record EventChoice(
    string? PromptText,
    string? PromptTextKey,
    IReadOnlyList<EventChoiceOption> Options
);
