namespace Spacer.Application.Events;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;

public sealed class EventPreconditionEvaluator
{
    public bool Evaluate(IReadOnlyList<EventCondition>? conditions, EventRenderContext context)
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

    private bool Evaluate(EventCondition condition, EventRenderContext context)
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

        return EvaluateLeaf(condition.Type, condition.Args, context.Variables);
    }

    private static bool EvaluateLeaf(string type, Dictionary<string, JsonElement>? args, IReadOnlyDictionary<string, string> variables)
    {
        var normalized = type.Trim();
        switch (normalized.ToLowerInvariant())
        {
            case "varexists":
                return TryGetKey(args, out var key) && variables.ContainsKey(key);
            case "varnotexists":
                return TryGetKey(args, out var missingKey) && !variables.ContainsKey(missingKey);
            case "varequals":
                return CompareEquality(args, variables, equals: true);
            case "varnotequals":
                return CompareEquality(args, variables, equals: false);
            case "vargreaterthan":
                return CompareNumeric(args, variables, (left, right) => left > right);
            case "vargreaterorequal":
                return CompareNumeric(args, variables, (left, right) => left >= right);
            case "varlessthan":
                return CompareNumeric(args, variables, (left, right) => left < right);
            case "varlessorequal":
                return CompareNumeric(args, variables, (left, right) => left <= right);
            case "varcontains":
                return CompareContains(args, variables);
            case "varin":
                return CompareIn(args, variables);
            default:
                return false;
        }
    }

    private static bool CompareEquality(Dictionary<string, JsonElement>? args, IReadOnlyDictionary<string, string> variables, bool equals)
    {
        if (!TryGetKey(args, out var key))
        {
            return false;
        }

        if (!TryGetValue(args, out var expected))
        {
            return false;
        }

        if (!variables.TryGetValue(key, out var actual))
        {
            actual = string.Empty;
        }

        var result = StringEquals(actual, expected);
        return equals ? result : !result;
    }

    private static bool CompareNumeric(Dictionary<string, JsonElement>? args, IReadOnlyDictionary<string, string> variables, Func<double, double, bool> comparator)
    {
        if (!TryGetKey(args, out var key))
        {
            return false;
        }

        if (!TryGetNumber(args, out var expected))
        {
            return false;
        }

        if (!variables.TryGetValue(key, out var actualRaw))
        {
            return false;
        }

        if (!double.TryParse(actualRaw, NumberStyles.Any, CultureInfo.InvariantCulture, out var actual))
        {
            return false;
        }

        return comparator(actual, expected);
    }

    private static bool CompareContains(Dictionary<string, JsonElement>? args, IReadOnlyDictionary<string, string> variables)
    {
        if (!TryGetKey(args, out var key))
        {
            return false;
        }

        if (!TryGetValue(args, out var expected))
        {
            return false;
        }

        if (!variables.TryGetValue(key, out var actual))
        {
            return false;
        }

        return actual.IndexOf(expected, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private static bool CompareIn(Dictionary<string, JsonElement>? args, IReadOnlyDictionary<string, string> variables)
    {
        if (!TryGetKey(args, out var key))
        {
            return false;
        }

        if (!variables.TryGetValue(key, out var actual))
        {
            return false;
        }

        if (args is null || !args.TryGetValue("values", out var valuesElement) || valuesElement.ValueKind != JsonValueKind.Array)
        {
            return false;
        }

        foreach (var element in valuesElement.EnumerateArray())
        {
            var expected = NormalizeValue(element);
            if (StringEquals(actual, expected))
            {
                return true;
            }
        }

        return false;
    }

    private static bool StringEquals(string actual, string expected)
    {
        if (double.TryParse(expected, NumberStyles.Any, CultureInfo.InvariantCulture, out var expectedNumber)
            && double.TryParse(actual, NumberStyles.Any, CultureInfo.InvariantCulture, out var actualNumber))
        {
            return Math.Abs(actualNumber - expectedNumber) < 0.000001;
        }

        if (bool.TryParse(expected, out var expectedBool)
            && bool.TryParse(actual, out var actualBool))
        {
            return expectedBool == actualBool;
        }

        return string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase);
    }

    private static bool TryGetKey(Dictionary<string, JsonElement>? args, out string key)
    {
        key = string.Empty;
        if (args is null || !args.TryGetValue("key", out var keyElement))
        {
            return false;
        }

        key = NormalizeValue(keyElement);
        return !string.IsNullOrWhiteSpace(key);
    }

    private static bool TryGetValue(Dictionary<string, JsonElement>? args, out string value)
    {
        value = string.Empty;
        if (args is null || !args.TryGetValue("value", out var valueElement))
        {
            return false;
        }

        value = NormalizeValue(valueElement);
        return true;
    }

    private static bool TryGetNumber(Dictionary<string, JsonElement>? args, out double value)
    {
        value = 0;
        if (!TryGetValue(args, out var raw))
        {
            return false;
        }

        return double.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
    }

    private static string NormalizeValue(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString() ?? string.Empty,
            JsonValueKind.Number => element.ToString(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            _ => element.ToString()
        };
    }
}
