namespace Spacer.Infrastructure.Events;

using System;
using System.Text.Json;
using Spacer.Application.Events;
using Spacer.Application.Ports;

/// <summary>
/// Handles the SetFlag action - sets a boolean state flag.
/// </summary>
public sealed class SetFlagActionHandler : IEventActionHandler
{
    public string Action => "SetFlag";

    public void Execute(EventStep step, EventContext context)
    {
        if (string.IsNullOrWhiteSpace(step.Flag))
        {
            return;
        }

        var value = step.Value ?? true;
        context.StateStore.SetFlag(step.Flag, value);
    }
}

/// <summary>
/// Handles the SetVar action - sets a variable value.
/// </summary>
public sealed class SetVarActionHandler : IEventActionHandler
{
    public string Action => "SetVar";

    public void Execute(EventStep step, EventContext context)
    {
        if (string.IsNullOrWhiteSpace(step.Flag))
        {
            return;
        }

        var value = step.Track;
        context.StateStore.SetVar(step.Flag, value);
    }
}

/// <summary>
/// Handles the AddVar action - adds to a variable value.
/// </summary>
public sealed class AddVarActionHandler : IEventActionHandler
{
    public string Action => "AddVar";

    public void Execute(EventStep step, EventContext context)
    {
        if (string.IsNullOrWhiteSpace(step.Flag))
        {
            return;
        }

        if (!step.Track.TryGetInt32(out var delta))
        {
            return;
        }

        var current = context.StateStore.GetVar(step.Flag);
        if (current.TryGetInt32(out var currentValue))
        {
            var newValue = currentValue + delta;
            context.StateStore.SetVar(step.Flag, JsonSerializer.SerializeToElement(newValue));
        }
    }
}
