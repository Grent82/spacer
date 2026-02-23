namespace Spacer.Infrastructure.Persistence;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Spacer.Application.Ports;
using Spacer.Domain.Enums;
using Spacer.Domain.Services;
using Spacer.Domain.ValueObjects;

public sealed class CsvNamePool : INamePool
{
    private readonly List<NameEntry> _entries;

    public CsvNamePool(string path)
    {
        _entries = LoadEntries(path);
    }

    public string? GetRandomName(Sex sex, EntityId factionId, IRandomSource random)
    {
        if (_entries.Count == 0)
        {
            return null;
        }

        var factionCandidates = factionId.IsNone
            ? Array.Empty<NameEntry>()
            : _entries.Where(entry => entry.FactionId == factionId).ToArray();

        var pool = factionCandidates.Length > 0
            ? factionCandidates
            : _entries.Where(entry => entry.FactionId.IsNone).ToArray();

        if (pool.Length == 0)
        {
            pool = _entries.ToArray();
        }

        if (sex != Sex.Unknown)
        {
            var sexMatches = pool.Where(entry => entry.Sex == sex).ToArray();
            if (sexMatches.Length > 0)
            {
                pool = sexMatches;
            }
            else
            {
                var neutralMatches = pool.Where(entry => entry.Sex == Sex.Unknown).ToArray();
                if (neutralMatches.Length > 0)
                {
                    pool = neutralMatches;
                }
            }
        }

        if (pool.Length == 0)
        {
            return null;
        }

        var index = random.Next(pool.Length);
        return pool[index].Name;
    }

    private static List<NameEntry> LoadEntries(string path)
    {
        var rows = CsvRowReader.ReadRows(path).ToList();
        if (rows.Count == 0)
        {
            return new List<NameEntry>();
        }

        var header = rows[0];
        var hasHeader = header.Any(cell => cell.Any(char.IsLetter));
        var map = hasHeader ? BuildHeaderMap(header) : BuildDefaultMap();
        if (hasHeader && !HasRequiredKeys(map))
        {
            hasHeader = false;
            map = BuildDefaultMap();
        }

        var entries = new List<NameEntry>();
        var startIndex = hasHeader ? 1 : 0;
        for (var rowIndex = startIndex; rowIndex < rows.Count; rowIndex++)
        {
            var row = rows[rowIndex];
            if (row.Length == 0)
            {
                continue;
            }

            var name = GetString(row, map, "name", string.Empty);
            if (string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            var sex = ParseSex(GetString(row, map, "sex", string.Empty));
            var factionId = GetInt(row, map, "factionid", 0);
            entries.Add(new NameEntry(name.Trim(), sex, factionId > 0 ? EntityId.Create(factionId) : EntityId.None));
        }

        return entries;
    }

    private static string GetString(string[] row, Dictionary<string, int> map, string key, string defaultValue)
    {
        return map.TryGetValue(key, out var index) && index >= 0 && index < row.Length
            ? row[index].Trim()
            : defaultValue;
    }

    private static int GetInt(string[] row, Dictionary<string, int> map, string key, int defaultValue)
    {
        if (!map.TryGetValue(key, out var index))
        {
            return defaultValue;
        }

        if (index < 0 || index >= row.Length)
        {
            return defaultValue;
        }

        return int.TryParse(row[index].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var result)
            ? result
            : defaultValue;
    }

    private static Sex ParseSex(string value)
    {
        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var numeric)
            && Enum.IsDefined(typeof(Sex), numeric))
        {
            return (Sex)numeric;
        }

        if (Enum.TryParse(value, ignoreCase: true, out Sex parsed))
        {
            return parsed;
        }

        return Sex.Unknown;
    }

    private static Dictionary<string, int> BuildHeaderMap(string[] header)
    {
        var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < header.Length; i++)
        {
            var key = Normalize(header[i]);
            if (key.Length == 0)
            {
                continue;
            }
            if (!map.ContainsKey(key))
            {
                map.Add(key, i);
            }
        }

        return map;
    }

    private static Dictionary<string, int> BuildDefaultMap()
    {
        return new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["name"] = 0,
            ["sex"] = 1,
            ["factionid"] = 2
        };
    }

    private static bool HasRequiredKeys(Dictionary<string, int> map)
    {
        return map.ContainsKey("name");
    }

    private static string Normalize(string value)
    {
        return value.Trim().Replace(" ", string.Empty).ToLowerInvariant();
    }

    private readonly record struct NameEntry(string Name, Sex Sex, EntityId FactionId);
}
