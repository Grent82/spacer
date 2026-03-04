namespace Spacer.Application.Events;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public sealed class EventRenderContext
{
    public EventRenderContext(IReadOnlyDictionary<string, string>? variables = null)
    {
        Variables = variables is null
            ? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, string>(variables, StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyDictionary<string, string> Variables { get; }
}

public sealed class EventRenderer
{
    public EventRenderResult Render(EventDefinition definition, EventRenderContext context)
    {
        var lines = new List<EventRenderLine>();
        var result = RenderSteps(definition.Steps, context, lines);
        return result ?? new EventRenderResult(true, lines, null);
    }

    public EventRenderResult RenderChoice(EventDefinition definition, EventRenderContext context, int choiceIndex)
    {
        var choiceStep = FindFirstChoice(definition.Steps);
        if (choiceStep is null)
        {
            return EventRenderResult.Empty;
        }

        var options = choiceStep.Options ?? Array.Empty<EventChoiceOption>();
        if (choiceIndex < 0 || choiceIndex >= options.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(choiceIndex));
        }

        var lines = new List<EventRenderLine>();
        RenderSteps(options[choiceIndex].Steps, context, lines);
        return new EventRenderResult(true, lines, null);
    }

    private EventRenderResult? RenderSteps(IReadOnlyList<EventStep> steps, EventRenderContext context, List<EventRenderLine> lines)
    {
        foreach (var step in steps)
        {
            switch (step.Type)
            {
                case "Message":
                    lines.Add(new EventRenderLine(
                        "Message",
                        ResolveText(step.Text, step.TextKey, context.Variables),
                        ResolveSpeaker(step.Speaker, context.Variables)));
                    break;
                case "Title":
                    lines.Add(new EventRenderLine(
                        "Title",
                        ResolveText(step.Text, step.TextKey, context.Variables),
                        null));
                    break;
                case "Choice":
                    var choice = BuildChoice(step, context);
                    return new EventRenderResult(false, lines, choice);
            }
        }

        return null;
    }

    private static EventRenderChoice BuildChoice(EventStep step, EventRenderContext context)
    {
        var prompt = ResolveText(step.PromptText, step.PromptTextKey, context.Variables);
        var options = step.Options ?? Array.Empty<EventChoiceOption>();
        var renderedOptions = options
            .Select((option, index) => new EventRenderChoiceOption(index, ResolveText(option.Text, option.TextKey, context.Variables)))
            .ToList();

        return new EventRenderChoice(prompt, renderedOptions);
    }

    private static EventStep? FindFirstChoice(IReadOnlyList<EventStep> steps)
    {
        foreach (var step in steps)
        {
            if (string.Equals(step.Type, "Choice", StringComparison.OrdinalIgnoreCase))
            {
                return step;
            }
        }

        return null;
    }

    private static string ResolveText(string? text, string? textKey, IReadOnlyDictionary<string, string> variables)
    {
        var value = text ?? textKey ?? string.Empty;
        return ReplacePlaceholders(value, variables);
    }

    private static string? ResolveSpeaker(string? speaker, IReadOnlyDictionary<string, string> variables)
    {
        if (string.IsNullOrWhiteSpace(speaker))
        {
            return speaker;
        }

        var resolved = ReplacePlaceholders(speaker, variables);
        return string.IsNullOrWhiteSpace(resolved) ? null : resolved;
    }

    private static string ReplacePlaceholders(string input, IReadOnlyDictionary<string, string> variables)
    {
        if (string.IsNullOrEmpty(input) || variables.Count == 0)
        {
            return input;
        }

        var builder = new StringBuilder(input.Length);
        for (var index = 0; index < input.Length; index++)
        {
            var current = input[index];
            if (current == '{')
            {
                var end = input.IndexOf('}', index + 1);
                if (end > index + 1)
                {
                    var key = input.Substring(index + 1, end - index - 1);
                    if (variables.TryGetValue(key, out var value))
                    {
                        builder.Append(value);
                    }
                    else
                    {
                        builder.Append('{').Append(key).Append('}');
                    }
                    index = end;
                    continue;
                }
            }

            builder.Append(current);
        }

        return builder.ToString();
    }
}
