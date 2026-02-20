namespace Spacer.Infrastructure.Persistence;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Spacer.Application.Ports;
using Spacer.Domain.Entities;
using Spacer.Domain.Enums;
using Spacer.Domain.ValueObjects;

public sealed class CsvPlanetRepository : IPlanetRepository
{
    private const int MaxPlanets = 50;
    private readonly Dictionary<EntityId, Planet> _byId;
    private readonly IReadOnlyList<Planet> _all;

    public CsvPlanetRepository(string path)
    {
        var list = new List<Planet>();
        var rows = CsvRowReader.ReadRows(path).ToList();
        if (rows.Count == 0)
        {
            _all = list;
            _byId = new Dictionary<EntityId, Planet>();
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

        for (var rowIndex = startIndex; rowIndex < rows.Count; rowIndex++)
        {
            if (list.Count >= MaxPlanets)
            {
                break;
            }

            var row = rows[rowIndex];
            if (row.Length == 0)
            {
                continue;
            }

            var id = GetInt(row, map, "id", -1);
            if (id <= 0)
            {
                continue;
            }

            var name = GetString(row, map, "name", $"Planet {id}");
            var x = GetInt(row, map, "x", 0);
            var y = GetInt(row, map, "y", 0);
            var planet = Planet.Create(id, name, new Position(x, y));

            var typeCode = GetInt(row, map, "typecode", 0);
            var planetType = ParsePlanetType(typeCode);
            planet.SetType(planetType, planetType == PlanetType.Capital);

            var systemId = GetInt(row, map, "systemid", 0);
            if (systemId > 0)
            {
                planet.SetSystemId(systemId);
            }

            var ownerFactionId = GetInt(row, map, "ownerfactionid", 0);
            if (ownerFactionId > 0)
            {
                planet.SetOwnerFaction(ownerFactionId);
            }

            var landBattleNumber = GetInt(row, map, "landbattlenumber", 0);
            var groundAttack = GetInt(row, map, "groundattack", 0);
            var groundDefense = GetInt(row, map, "grounddefense", 0);
            planet.ConfigureDefense(landBattleNumber, groundAttack, groundDefense);

            var population = GetInt(row, map, "population", 0);
            var production = GetInt(row, map, "production", 0);
            var publicOpinion = GetInt(row, map, "publicopinion", 50);
            var loyalty = GetInt(row, map, "loyalty", 50);
            var gold = GetInt(row, map, "gold", 0);
            planet.ConfigureEconomy(population, production, publicOpinion, loyalty, gold);

            list.Add(planet);
        }

        _all = list;
        _byId = new Dictionary<EntityId, Planet>(list.Count);
        foreach (var planet in list)
        {
            _byId[planet.Id] = planet;
        }
    }

    public IReadOnlyList<Planet> GetAll() => _all;

    public Planet? FindById(EntityId id)
    {
        return _byId.TryGetValue(id, out var planet) ? planet : null;
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

        MapIfPresent(map, "owner", "ownerfactionid");
        MapIfPresent(map, "faction", "ownerfactionid");
        MapIfPresent(map, "factionid", "ownerfactionid");
        MapIfPresent(map, "type", "typecode");
        MapIfPresent(map, "planettype", "typecode");
        MapIfPresent(map, "capital", "typecode");
        MapIfPresent(map, "money", "gold");
        MapIfPresent(map, "funds", "gold");
        MapIfPresent(map, "pop", "population");
        MapIfPresent(map, "public", "publicopinion");
        MapIfPresent(map, "opinion", "publicopinion");
        MapIfPresent(map, "citizensloyalty", "loyalty");
        MapIfPresent(map, "posx", "x");
        MapIfPresent(map, "posy", "y");

        return map;
    }

    private static Dictionary<string, int> BuildDefaultMap()
    {
        return new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["id"] = 0,
            ["name"] = 1,
            ["ownerfactionid"] = 2,
            ["gold"] = 3,
            ["typecode"] = 4,
            ["landbattlenumber"] = 6,
            ["systemid"] = 7,
            ["population"] = 8,
            ["x"] = 10,
            ["y"] = 11
        };
    }

    private static bool HasRequiredKeys(Dictionary<string, int> map)
    {
        return map.ContainsKey("id") && map.ContainsKey("name");
    }

    private static PlanetType ParsePlanetType(int value)
    {
        return value switch
        {
            1 => PlanetType.Colony,
            2 => PlanetType.Fortress,
            3 => PlanetType.Capital,
            _ => PlanetType.Unknown
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
}
