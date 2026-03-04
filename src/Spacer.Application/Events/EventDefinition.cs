namespace Spacer.Application.Events;

using System;
using System.Collections.Generic;
using System.Text.Json;

public sealed record EventDefinition
{
    public string Id { get; init; } = string.Empty;
    public string Trigger { get; init; } = string.Empty;
    public int Version { get; init; } = 1;
    public bool Once { get; init; }
    public int Priority { get; init; }
    public int? CooldownTurns { get; init; }
    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();
    public IReadOnlyList<EventCondition>? Conditions { get; init; }
    public IReadOnlyList<EventStep> Steps { get; init; } = Array.Empty<EventStep>();
    public string? Source { get; init; }
}

public sealed record EventCondition
{
    public IReadOnlyList<EventCondition>? All { get; init; }
    public IReadOnlyList<EventCondition>? Any { get; init; }
    public EventCondition? Not { get; init; }
    public string? Type { get; init; }
    public Dictionary<string, JsonElement>? Args { get; init; }
}

public sealed record EventStep
{
    public string Type { get; init; } = string.Empty;
    public string? Text { get; init; }
    public string? TextKey { get; init; }
    public string? Speaker { get; init; }
    public string? PromptText { get; init; }
    public string? PromptTextKey { get; init; }
    public bool Shuffle { get; init; }
    public IReadOnlyList<EventChoiceOption>? Options { get; init; }
    public string? Action { get; init; }
    public Dictionary<string, JsonElement>? Args { get; init; }
    public string? Flag { get; init; }
    public bool? Value { get; init; }
    public JsonElement Track { get; init; }
    public EventCondition? When { get; init; }
}

public sealed record EventChoiceOption
{
    public string? Text { get; init; }
    public string? TextKey { get; init; }
    public IReadOnlyList<EventStep> Steps { get; init; } = Array.Empty<EventStep>();
}
