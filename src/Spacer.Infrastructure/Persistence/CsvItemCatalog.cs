namespace Spacer.Infrastructure.Persistence;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Spacer.Application.Ports;
using Spacer.Domain.Entities;
using Spacer.Domain.Enums;

public sealed class CsvItemCatalog : IItemCatalog
{
    private const int MaxItems = 200;
    private readonly string _csvPath;
    private readonly Encoding? _encoding;
    private readonly List<ItemSpec> _cache = new();
    private bool _loaded;

    public CsvItemCatalog(string csvPath, Encoding? encoding = null)
    {
        _csvPath = csvPath ?? string.Empty;
        _encoding = encoding;
    }

    public IReadOnlyList<ItemSpec> GetAll()
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

        var rows = CsvRowReader.ReadRows(_csvPath, _encoding).ToList();
        if (rows.Count == 0)
        {
            return;
        }

        var header = rows[0];
        var hasHeader = header.Any(cell => cell.Any(char.IsLetter));
        var map = hasHeader ? BuildHeaderMap(header) : BuildDefaultMap();
        if (hasHeader && !HasRequiredKeys(map))
        {
            hasHeader = false;
            map = BuildDefaultMap();
        }

        var startIndex = hasHeader ? 1 : 0;
        var nextId = 1;

        for (var rowIndex = startIndex; rowIndex < rows.Count; rowIndex++)
        {
            if (_cache.Count >= MaxItems)
            {
                return;
            }

            var row = rows[rowIndex];
            if (row.Length == 0)
            {
                continue;
            }

            var displayName = GetString(row, map, "name", string.Empty);
            var description = GetString(row, map, "description", string.Empty);

            if (string.IsNullOrWhiteSpace(displayName))
            {
                continue;
            }

            var effectCode = GetInt(row, map, "effectcode", 0);
            var effectValue = GetInt(row, map, "effectvalue", 0);
            var itemType = GetInt(row, map, "itemtype", 0);
            var distributionCount = GetInt(row, map, "distributioncount", 0);

            if (!TryParseEnum(effectCode, out ItemEffectType effectType))
            {
                continue;
            }

            if (!TryParseEnum(itemType, out ItemUsageType usageType))
            {
                continue;
            }

            if (distributionCount < 0)
            {
                continue;
            }

            var spec = ItemSpec.Create(nextId, displayName);
            spec.ConfigureDefinition( displayName, description, effectType, effectValue, usageType, distributionCount );

            _cache.Add(spec);
            nextId++;

            if (distributionCount > 1)
            {
                for (var copyIndex = 1; copyIndex < distributionCount; copyIndex++)
                {
                    if (_cache.Count >= MaxItems)
                    {
                        return;
                    }

                    var duplicate = ItemSpec.Create(nextId, displayName);
                    duplicate.ConfigureDefinition( displayName, description, effectType, effectValue, usageType, distributionCount );

                    _cache.Add(duplicate);
                    nextId++;
                }
            }
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

        MapIfPresent(map, "itemname", "name");
        MapIfPresent(map, "japanesename", "name");
        MapIfPresent(map, "description", "description");
        MapIfPresent(map, "effect", "effectcode");
        MapIfPresent(map, "effectitem", "effectcode");
        MapIfPresent(map, "effecttype", "effectcode");
        MapIfPresent(map, "effectvalue", "effectvalue");
        MapIfPresent(map, "value", "effectvalue");
        MapIfPresent(map, "type", "itemtype");
        MapIfPresent(map, "itemtype", "itemtype");
        MapIfPresent(map, "distribution", "distributioncount");
        MapIfPresent(map, "distributioncount", "distributioncount");

        return map;
    }

    private static Dictionary<string, int> BuildDefaultMap()
    {
        return new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["name"] = 0,
            ["description"] = 1,
            ["effectcode"] = 2,
            ["effectvalue"] = 3,
            ["itemtype"] = 4,
            ["distributioncount"] = 5,
        };
    }

    private static bool HasRequiredKeys(Dictionary<string, int> map)
    {
        return map.ContainsKey("name") || map.ContainsKey("description");
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

        return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
    }

    private static bool TryGetValue(string[] row, Dictionary<string, int> map, string key, out string value)
    {
        if (!map.TryGetValue(key, out var index))
        {
            value = string.Empty;
            return false;
        }

        value = index >= 0 && index < row.Length ? row[index].Trim() : string.Empty;
        return true;
    }

    private static bool TryParseEnum<TEnum>(int value, out TEnum parsed)
        where TEnum : struct, Enum
    {
        if (Enum.IsDefined(typeof(TEnum), value))
        {
            parsed = (TEnum)Enum.ToObject(typeof(TEnum), value);
            return true;
        }

        parsed = default;
        return false;
    }

}
