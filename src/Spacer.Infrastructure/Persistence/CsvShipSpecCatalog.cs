namespace Spacer.Infrastructure.Persistence;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Spacer.Application.Ports;
using Spacer.Domain.Entities;
using Spacer.Domain.ValueObjects;

public sealed class CsvShipSpecCatalog : IShipSpecCatalog
{
    private const int StatCount = ShipStatBlock.StatCount;

    private readonly string _csvPath;
    private readonly List<ShipSpec> _cache = new();
    private bool _loaded;

    public CsvShipSpecCatalog(string csvPath)
    {
        _csvPath = csvPath ?? string.Empty;
    }

    public IReadOnlyList<ShipSpec> GetAll()
    {
        EnsureLoaded();
        return _cache;
    }

    private void EnsureLoaded()
    {
        if (_loaded)
        {
            return;
        }
        _loaded = true;

        var rows = CsvRowReader.ReadRows(_csvPath).ToList();
        if (rows.Count == 0)
        {
            return;
        }

        var header = rows[0];
        var hasHeader = header.Any(cell => cell.Any(char.IsLetter));
        var map = hasHeader ? BuildHeaderMap(header) : BuildDefaultMap();
        var startIndex = hasHeader ? 1 : 0;

        for (var rowIndex = startIndex; rowIndex < rows.Count; rowIndex++)
        {
            var row = rows[rowIndex];
            if (row.Length == 0)
            {
                continue;
            }

            var id = GetInt(row, map, "id", -1);
            var name = GetString(row, map, "name", $"Ship {id}");
            var catalogIndex = GetInt(row, map, "catalogindex", rowIndex);
            var typeCode = GetInt(row, map, "typecode", -1);
            if (id < 0 || typeCode < 0)
            {
                continue;
            }

            var stats = new int[StatCount];
            for (var i = 0; i < StatCount; i++)
            {
                stats[i] = GetInt(row, map, $"stat{i}", 0);
            }

            var baseCost = GetInt(row, map, "basecost", 0);
            var carrierLoading = GetInt(row, map, "basecarrierloading", 0);
            var landingPodLoading = GetInt(row, map, "baselandingpodloading", 0);
            var shipLoading = GetInt(row, map, "baseshiploading", 0);

            var spec = ShipSpec.Create(id, name);
            spec.ConfigureDefinition(
                catalogIndex,
                typeCode,
                new ShipStatBlock(stats),
                baseCost,
                carrierLoading,
                landingPodLoading,
                shipLoading
            );

            _cache.Add(spec);
        }
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

        MapIfPresent(map, "attack", "stat0");
        MapIfPresent(map, "antiair", "stat1");
        MapIfPresent(map, "defense", "stat2");
        MapIfPresent(map, "minesweeping", "stat3");
        MapIfPresent(map, "construction", "stat4");
        MapIfPresent(map, "maneuvering", "stat5");
        MapIfPresent(map, "starfighterattack", "stat6");
        MapIfPresent(map, "starfighterdefense", "stat7");
        MapIfPresent(map, "groundattack", "stat8");
        MapIfPresent(map, "grounddefense", "stat9");
        MapIfPresent(map, "carrierloading", "basecarrierloading");
        MapIfPresent(map, "landingpodloading", "baselandingpodloading");
        MapIfPresent(map, "shiploading", "baseshiploading");

        return map;
    }

    private static Dictionary<string, int> BuildDefaultMap()
    {
        var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["id"] = 0,
            ["name"] = 1,
            ["catalogindex"] = 2,
            ["typecode"] = 3,
        };

        var offset = 4;
        for (var i = 0; i < StatCount; i++)
        {
            map[$"stat{i}"] = offset + i;
        }

        map["basecost"] = offset + StatCount;
        map["basecarrierloading"] = offset + StatCount + 1;
        map["baselandingpodloading"] = offset + StatCount + 2;
        map["baseshiploading"] = offset + StatCount + 3;

        return map;
    }

    private static string Normalize(string value)
    {
        var span = value.AsSpan().Trim();
        if (span.Length == 0)
        {
            return string.Empty;
        }

        var buffer = new char[span.Length];
        var length = 0;

        for (var i = 0; i < span.Length; i++)
        {
            var ch = span[i];
            if (char.IsLetterOrDigit(ch))
            {
                buffer[length++] = char.ToLowerInvariant(ch);
            }
        }

        return length == 0 ? string.Empty : new string(buffer, 0, length);
    }

    private static void MapIfPresent(
        Dictionary<string, int> map,
        string alias,
        string canonical
    )
    {
        if (!map.ContainsKey(canonical) && map.TryGetValue(alias, out var index))
        {
            map[canonical] = index;
        }
    }

    private static int GetInt(string[] row, Dictionary<string, int> map, string key, int fallback)
    {
        if (!TryGetValue(row, map, key, out var value))
        {
            return fallback;
        }

        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
        {
            return result;
        }

        return fallback;
    }

    private static string GetString(string[] row, Dictionary<string, int> map, string key, string fallback)
    {
        if (!TryGetValue(row, map, key, out var value))
        {
            return fallback;
        }

        return value.Length == 0 ? fallback : value;
    }

    private static bool TryGetValue(
        string[] row,
        Dictionary<string, int> map,
        string key,
        out string value
    )
    {
        value = string.Empty;
        if (!map.TryGetValue(key, out var index))
        {
            return false;
        }
        if (index < 0 || index >= row.Length)
        {
            return false;
        }

        value = row[index].Trim();
        return true;
    }
}
