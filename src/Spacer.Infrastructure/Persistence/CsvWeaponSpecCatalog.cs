namespace Spacer.Infrastructure.Persistence;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Spacer.Application.Ports;
using Spacer.Domain.Entities;

public sealed class CsvWeaponSpecCatalog : IWeaponSpecCatalog
{
    private readonly string _csvPath;
    private readonly IWeaponNameFormatter? _nameFormatter;
    private readonly List<WeaponSpec> _cache = new();
    private bool _loaded;

    public CsvWeaponSpecCatalog(string csvPath, IWeaponNameFormatter? nameFormatter = null)
    {
        _csvPath = csvPath ?? string.Empty;
        _nameFormatter = nameFormatter;
    }

    public IReadOnlyList<WeaponSpec> GetAll()
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
            var name = GetString(row, map, "name", $"Weapon {id}");
            var shipTypeCode = GetInt(row, map, "shiptypecode", -1);
            var factionId = GetInt(row, map, "factionid", -1);
            if (factionId < 0)
            {
                factionId = GetInt(row, map, "seriesid", -1);
            }
            if (factionId < 0)
            {
                factionId = GetInt(row, map, "systemid", -1);
            }

            var tier = GetInt(row, map, "tier", -1);
            if (tier < 0)
            {
                tier = GetInt(row, map, "stage", -1);
            }
            var dpmId = GetInt(row, map, "dpmid", 0);
            var imageKey = GetString(row, map, "imagekey", string.Empty);

            if (id < 0 || shipTypeCode <= 0 || factionId <= 0 || tier <= 0)
            {
                continue;
            }

            var finalName = string.IsNullOrWhiteSpace(name)
                ? _nameFormatter?.BuildName(factionId, shipTypeCode, tier) ?? $"Weapon {id}"
                : name;

            var finalImageKey = string.IsNullOrWhiteSpace(imageKey)
                ? _nameFormatter?.BuildImageKey(factionId, shipTypeCode, tier) ?? string.Empty
                : imageKey;

            var spec = WeaponSpec.Create(id, finalName);
            spec.ConfigureDefinition(
                shipTypeCode,
                factionId,
                tier,
                dpmId,
                finalImageKey
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

        return map;
    }

    private static Dictionary<string, int> BuildDefaultMap()
    {
        return new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["id"] = 0,
            ["name"] = 1,
            ["shiptypecode"] = 2,
            ["factionid"] = 3,
            ["tier"] = 4,
            ["dpmid"] = 5,
            ["imagekey"] = 6,
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
