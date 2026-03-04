namespace Spacer.Application.Events;

using System;
using System.Collections.Generic;

public sealed record EventRenderResult(
    bool Completed,
    IReadOnlyList<EventRenderLine> Lines,
    EventRenderChoice? PendingChoice
)
{
    public static readonly EventRenderResult Empty =
        new(true, Array.Empty<EventRenderLine>(), null);
}

public sealed record EventRenderLine(string Type, string Text, string? Speaker);

public sealed record EventRenderChoice(
    string? PromptText,
    IReadOnlyList<EventRenderChoiceOption> Options
);

public sealed record EventRenderChoiceOption(int Index, string Text);
