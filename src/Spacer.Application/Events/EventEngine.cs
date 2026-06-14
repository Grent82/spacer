namespace Spacer.Application.Events;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Spacer.Application.Ports;

public sealed class EventEngine
{
    private readonly IEventCatalog _catalog;
    private readonly EventConditionEvaluator _conditionEvaluator;
    private readonly EventRunner _runner;
    private readonly IEventStateStore _stateStore;

    public EventEngine( IEventCatalog catalog, EventConditionEvaluator conditionEvaluator, EventRunner runner, IEventStateStore stateStore )
    {
        _catalog = catalog;
        _conditionEvaluator = conditionEvaluator;
        _runner = runner;
        _stateStore = stateStore;
    }

    public IReadOnlyList<EventDefinition> GetCandidates(string trigger, EventContext context)
    {
        var candidates = _catalog.GetByTrigger(trigger)
            .Where(definition => !IsSuppressed(definition))
            .Where(definition => _conditionEvaluator.Evaluate(definition.Conditions, context))
            .OrderByDescending(definition => definition.Priority)
            .ToList();

        return candidates;
    }

    public EventExecutionResult? TryRun(string trigger, EventContext context)
    {
        var candidates = GetCandidates(trigger, context);
        if (candidates.Count == 0)
        {
            return null;
        }

        var selected = candidates[0];
        var result = _runner.Run(selected, context);
        if (result.Completed)
        {
            MarkConsumed(selected);
        }
        return result;
    }

    private bool IsSuppressed(EventDefinition definition)
    {
        if (definition.Once && _stateStore.GetFlag(GetOnceFlag(definition.Id)))
        {
            return true;
        }

        if (definition.CooldownTurns is > 0 && _stateStore.GetCooldownRemaining(definition.Id) > 0)
        {
            return true;
        }

        return false;
    }

    private void MarkConsumed(EventDefinition definition)
    {
        if (definition.Once)
        {
            _stateStore.SetFlag(GetOnceFlag(definition.Id), true);
        }

        if (definition.CooldownTurns is > 0)
        {
            _stateStore.SetCooldown(definition.Id, definition.CooldownTurns.Value);
        }
    }

    private static string GetOnceFlag(string id) => $"event:{id}:once";
}

public sealed class EventConditionEvaluator
{
    private readonly Dictionary<string, IEventConditionHandler> _handlers;

    public EventConditionEvaluator(IEnumerable<IEventConditionHandler> handlers)
    {
        _handlers = handlers?.ToDictionary(handler => handler.Type, StringComparer.OrdinalIgnoreCase)
            ?? new Dictionary<string, IEventConditionHandler>(StringComparer.OrdinalIgnoreCase);
    }

    public bool Evaluate(IReadOnlyList<EventCondition>? conditions, EventContext context)
    {
        if (conditions is null || conditions.Count == 0)
        {
            return true;
        }

        foreach (var condition in conditions)
        {
            if (!Evaluate(condition, context))
            {
                return false;
            }
        }

        return true;
    }

    public bool Evaluate(EventCondition condition, EventContext context)
    {
        if (condition.All is { Count: > 0 })
        {
            return condition.All.All(item => Evaluate(item, context));
        }

        if (condition.Any is { Count: > 0 })
        {
            return condition.Any.Any(item => Evaluate(item, context));
        }

        if (condition.Not is not null)
        {
            return !Evaluate(condition.Not, context);
        }

        if (string.IsNullOrWhiteSpace(condition.Type))
        {
            return true;
        }

        if (!_handlers.TryGetValue(condition.Type, out var handler))
        {
            throw new InvalidOperationException($"Unknown condition type '{condition.Type}'.");
        }

        return handler.Evaluate(condition, context);
    }
}

public interface IEventConditionHandler
{
    string Type { get; }
    bool Evaluate(EventCondition condition, EventContext context);
}

public sealed class EventRunner
{
    private readonly EventActionExecutor _actionExecutor;
    private readonly EventConditionEvaluator _conditionEvaluator;

    public EventRunner(EventActionExecutor actionExecutor, EventConditionEvaluator conditionEvaluator)
    {
        _actionExecutor = actionExecutor;
        _conditionEvaluator = conditionEvaluator;
    }

    public EventExecutionResult Run(EventDefinition definition, EventContext context)
    {
        var log = new List<EventLogEntry>();
        var result = ExecuteSteps(definition.Steps, context, log);
        return result ?? new EventExecutionResult(true, log, null);
    }

    private EventExecutionResult? ExecuteSteps( IReadOnlyList<EventStep> steps, EventContext context, List<EventLogEntry> log )
    {
        foreach (var step in steps)
        {
            if (step.When is not null)
            {
                if (!_conditionEvaluator.Evaluate(step.When, context))
                {
                    continue;
                }
            }

            switch (step.Type)
            {
                case "Message":
                    log.Add(new EventLogEntry("Message", step.Text ?? step.TextKey ?? string.Empty));
                    break;
                case "Title":
                    log.Add(new EventLogEntry("Title", step.Text ?? step.TextKey ?? string.Empty));
                    break;
                case "Bgm":
                    var trackValue = step.Track.ValueKind == JsonValueKind.Undefined
                        ? string.Empty
                        : step.Track.ValueKind == JsonValueKind.String
                            ? step.Track.GetString() ?? string.Empty
                            : step.Track.ToString();
                    log.Add(new EventLogEntry("Bgm", trackValue));
                    break;
                case "SetFlag":
                    if (!string.IsNullOrWhiteSpace(step.Flag) && step.Value.HasValue)
                    {
                        context.StateStore.SetFlag(step.Flag, step.Value.Value);
                    }
                    break;
                case "Action":
                    _actionExecutor.Execute(step, context);
                    break;
                case "Choice":
                    var choice = new EventChoice(step.PromptText, step.PromptTextKey, step.Options ?? Array.Empty<EventChoiceOption>());
                    return new EventExecutionResult(false, log, choice);
            }
        }

        return null;
    }
}

public sealed class EventActionExecutor
{
    private readonly Dictionary<string, IEventActionHandler> _handlers;

    public EventActionExecutor(IEnumerable<IEventActionHandler> handlers)
    {
        _handlers = handlers?.ToDictionary(handler => handler.Action, StringComparer.OrdinalIgnoreCase)
            ?? new Dictionary<string, IEventActionHandler>(StringComparer.OrdinalIgnoreCase);
    }

    public void Execute(EventStep step, EventContext context)
    {
        if (string.IsNullOrWhiteSpace(step.Action))
        {
            return;
        }

        if (!_handlers.TryGetValue(step.Action, out var handler))
        {
            throw new InvalidOperationException($"Unknown action '{step.Action}'.");
        }

        handler.Execute(step, context);
    }
}

public interface IEventActionHandler
{
    string Action { get; }
    void Execute(EventStep step, EventContext context);
}
