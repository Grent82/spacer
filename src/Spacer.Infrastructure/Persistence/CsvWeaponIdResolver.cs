namespace Spacer.Infrastructure.Persistence;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Spacer.Application.Ports;

public sealed class CsvWeaponIdResolver : IWeaponIdResolver
{
    private readonly string _csvPath;
    private readonly Dictionary<WeaponKey, int> _table = new();
    private readonly List<WeaponRule> _rules = new();
    private bool _loaded;

    public CsvWeaponIdResolver(string csvPath)
    {
        _csvPath = csvPath ?? string.Empty;
    }

    public int ResolveWeaponId(int factionId, int tier, int shipTypeCode)
    {
        EnsureLoaded();

        var key = new WeaponKey(factionId, tier, shipTypeCode);
        if (_table.TryGetValue(key, out var value))
        {
            return value;
        }

        for (var i = 0; i < _rules.Count; i++)
        {
            if (_rules[i].Matches(factionId, tier, shipTypeCode))
            {
                return _rules[i].WeaponId;
            }
        }

        return 0;
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

            var factionId = GetInt(row, map, "factionid", -1);
            if (factionId < 0)
            {
                factionId = GetInt(row, map, "seriesid", -1);
            }
            if (factionId < 0)
            {
                factionId = GetInt(row, map, "systemid", 0);
            }

            var tier = GetInt(row, map, "tier", -1);
            if (tier < 0)
            {
                tier = GetInt(row, map, "stage", 0);
            }
            var shipType = GetInt(row, map, "shiptypecode", 0);
            var weaponId = GetInt(row, map, "weaponid", 0);

            if (factionId < 0 || tier < 0 || shipType < 0)
            {
                _rules.Add(new WeaponRule(factionId, tier, shipType, weaponId));
                continue;
            }

            var key = new WeaponKey(factionId, tier, shipType);
            _table[key] = weaponId;
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

        return map;
    }

    private static Dictionary<string, int> BuildDefaultMap()
    {
        return new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["factionid"] = 0,
            ["tier"] = 1,
            ["shiptypecode"] = 2,
            ["weaponid"] = 3,
        };
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

    private static int GetInt(string[] row, Dictionary<string, int> map, string key, int fallback)
    {
        if (!map.TryGetValue(key, out var index))
        {
            return fallback;
        }
        if (index < 0 || index >= row.Length)
        {
            return fallback;
        }

        var value = row[index].Trim();
        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
        {
            return result;
        }

        return fallback;
    }

    private readonly record struct WeaponKey(int FactionId, int Tier, int ShipTypeCode);

    private readonly record struct WeaponRule(int FactionId, int Tier, int ShipTypeCode, int WeaponId)
    {
        public bool Matches(int factionId, int tier, int shipTypeCode)
        {
            return (FactionId < 0 || FactionId == factionId)
                && (Tier < 0 || Tier == tier)
                && (ShipTypeCode < 0 || ShipTypeCode == shipTypeCode);
        }
    }
}
