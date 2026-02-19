namespace Spacer.Infrastructure.Persistence;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Spacer.Application.DTOs;
using Spacer.Application.Ports;

public sealed class CsvFactionCatalog : IFactionCatalog
{
    private readonly string _csvPath;
    private readonly Dictionary<int, FactionInfo> _cache = new();
    private bool _loaded;

    public CsvFactionCatalog(string csvPath)
    {
        _csvPath = csvPath ?? string.Empty;
    }

    public FactionInfo? FindById(int id)
    {
        EnsureLoaded();
        return _cache.TryGetValue(id, out var info) ? info : null;
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
            var code = GetString(row, map, "code", string.Empty);
            var name = GetString(row, map, "name", string.Empty);

            if (id <= 0)
            {
                continue;
            }

            _cache[id] = new FactionInfo(id, code, name);
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
            ["code"] = 1,
            ["name"] = 2,
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
