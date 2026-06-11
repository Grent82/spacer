namespace Spacer.Infrastructure.Events;

using System;
using System.Text.Json;
using Spacer.Application.Events;
using Spacer.Application.Ports;

/// <summary>
/// Handles the VarEquals condition - checks if a variable equals a value.
/// </summary>
public sealed class VarEqualsConditionHandler : IEventConditionHandler
{
    public string Type => "VarEquals";

    public bool Evaluate(EventCondition condition, EventContext context)
    {
        if (condition.Args is not { Count: > 0 })
        {
            return false;
        }

        if (!condition.Args.TryGetValue("key", out var keyElement) ||
            !condition.Args.TryGetValue("value", out var valueElement))
        {
            return false;
        }

        var key = keyElement.GetString();
        if (string.IsNullOrWhiteSpace(key))
        {
            return false;
        }

        var currentValue = context.StateStore.GetVar(key);

        // Handle boolean comparison.
        if (valueElement.ValueKind == JsonValueKind.True || valueElement.ValueKind == JsonValueKind.False)
        {
            var expected = valueElement.GetBoolean();
            var actual = currentValue.GetBoolean();
            return actual == expected;
        }

        // Handle integer comparison.
        if (valueElement.TryGetInt32(out var expectedInt))
        {
            if (currentValue.TryGetInt32(out var actualInt))
            {
                return actualInt == expectedInt;
            }
            // Try parsing from string.
            var actualStr = currentValue.GetString();
            if (!string.IsNullOrEmpty(actualStr) && int.TryParse(actualStr, out var parsed))
            {
                return parsed == expectedInt;
            }
            return false;
        }

        // Handle string comparison.
        var expectedStr = valueElement.GetString();
        if (!string.IsNullOrEmpty(expectedStr))
        {
            var actualStr = currentValue.GetString();
            if (!string.IsNullOrEmpty(actualStr))
            {
                return string.Equals(actualStr, expectedStr, StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        return false;
    }
}

/// <summary>
/// Handles the VarGte condition - checks if a variable is greater than or equal to a value.
/// </summary>
public sealed class VarGteConditionHandler : IEventConditionHandler
{
    public string Type => "VarGte";

    public bool Evaluate(EventCondition condition, EventContext context)
    {
        if (condition.Args is not { Count: > 0 })
        {
            return false;
        }

        if (!condition.Args.TryGetValue("key", out var keyElement) ||
            !condition.Args.TryGetValue("value", out var valueElement))
        {
            return false;
        }

        var key = keyElement.GetString();
        if (string.IsNullOrWhiteSpace(key))
        {
            return false;
        }

        var currentValue = context.StateStore.GetVar(key);

        if (valueElement.TryGetInt32(out var expectedInt))
        {
            if (currentValue.TryGetInt32(out var actualInt))
            {
                return actualInt >= expectedInt;
            }
            var actualStr = currentValue.GetString();
            if (!string.IsNullOrEmpty(actualStr) && int.TryParse(actualStr, out var parsed))
            {
                return parsed >= expectedInt;
            }
            return false;
        }

        return false;
    }
}

/// <summary>
/// Handles the VarExists condition - checks if a variable exists and is not empty.
/// </summary>
public sealed class VarExistsConditionHandler : IEventConditionHandler
{
    public string Type => "VarExists";

    public bool Evaluate(EventCondition condition, EventContext context)
    {
        if (condition.Args is not { Count: > 0 })
        {
            return false;
        }

        if (!condition.Args.TryGetValue("key", out var keyElement))
        {
            return false;
        }

        var key = keyElement.GetString();
        if (string.IsNullOrWhiteSpace(key))
        {
            return false;
        }

        var value = context.StateStore.GetVar(key);

        // Check if value exists and is not default.
        if (value.ValueKind == JsonValueKind.Undefined || value.ValueKind == JsonValueKind.Null)
        {
            return false;
        }

        if (value.ValueKind == JsonValueKind.True)
        {
            return true;
        }

        if (value.TryGetInt32(out var intVal))
        {
            return intVal > 0;
        }

        var strVal = value.GetString();
        if (!string.IsNullOrWhiteSpace(strVal))
        {
            return true;
        }

        return false;
    }
}
