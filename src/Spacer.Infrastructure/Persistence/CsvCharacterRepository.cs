namespace Spacer.Infrastructure.Persistence;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Spacer.Application.Ports;
using Spacer.Domain.Entities;
using Spacer.Domain.Enums;
using Spacer.Domain.ValueObjects;

public sealed class CsvCharacterRepository : ICharacterRepository, ICharacterRoster
{
    private readonly Dictionary<EntityId, Character> _byId;
    private readonly IReadOnlyList<Character> _all;

    public CsvCharacterRepository(IEnumerable<string> characterPaths, FactionPoliticsRules rules)
    {
        var list = new List<Character>();
        var knownIds = new HashSet<int>();

        foreach (var characterPath in characterPaths)
        {
            LoadCharacters(characterPath, list, knownIds, rules);
        }

        _all = list;
        _byId = new Dictionary<EntityId, Character>(list.Count);
        foreach (var character in list)
        {
            _byId[character.Id] = character;
        }
    }

    public IReadOnlyList<Character> GetAll() => _all;

    public Character? FindById(EntityId id)
    {
        return _byId.TryGetValue(id, out var character) ? character : null;
    }

    private static string GetCell(string[] row, int index)
    {
        return index >= 0 && index < row.Length ? row[index].Trim() : string.Empty;
    }

    private static void LoadCharacters( string path, List<Character> list, HashSet<int> knownIds, FactionPoliticsRules rules)
    {
        var rows = CsvRowReader.ReadRows(path).ToList();
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

        for (var rowIndex = startIndex; rowIndex < rows.Count; rowIndex++)
        {
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

            if (!knownIds.Add(id))
            {
                continue;
            }

            var name = GetString(row, map, "name", $"Character {id}");

            var sex = ParseSex(GetString(row, map, "sex", string.Empty));
            var character = Character.Create(id, name, sex);

            character.SetState(ParseState(GetString(row, map, "state", string.Empty)));
            character.SetAge(GetInt(row, map, "age", 0));
            character.SetLoyalty(GetInt(row, map, "loyalty", 0), rules);
            character.SetMerits(GetInt(row, map, "merits", 0));
            character.SetRank(GetInt(row, map, "rank", character.Merits));

            var factionId = GetInt(row, map, "factionid", 0);
            var overlordId = GetInt(row, map, "overlordid", 0);
            if (factionId > 0 || overlordId > 0)
            {
                var factionEntityId = factionId > 0 ? EntityId.Create(factionId) : EntityId.None;
                var overlordEntityId = overlordId > 0 ? EntityId.Create(overlordId) : EntityId.None;
                character.SetFaction(factionEntityId, overlordEntityId);
            }

            var fatherId = GetInt(row, map, "fatherid", 0);
            var motherId = GetInt(row, map, "motherid", 0);
            var partnerId = GetInt(row, map, "partnerid", 0);
            if (fatherId > 0 || motherId > 0 || partnerId > 0)
            {
                character.SetFamily(
                    fatherId > 0 ? EntityId.Create(fatherId) : EntityId.None,
                    motherId > 0 ? EntityId.Create(motherId) : EntityId.None,
                    partnerId > 0 ? EntityId.Create(partnerId) : EntityId.None
                );
            }

            character.SetFriendshipIntimacy(GetInt(row, map, "friendshipintimacy", 0));
            character.SetPersonality(ParsePersonality(GetString(row, map, "personality", string.Empty)));

            var attack = GetInt(row, map, "attack", 0);
            var defense = GetInt(row, map, "defense", 0);
            var intelligence = GetInt(row, map, "intelligence", 0);
            var strength = GetInt(row, map, "strength", 0);
            var charisma = GetInt(row, map, "charisma", 0);
            var cleverness = GetInt(row, map, "cleverness", 0);
            var battle = GetInt(row, map, "battle", strength);
            var diplomacy = GetInt(row, map, "diplomacy", 0);

            if (attack != 0 || defense != 0 || intelligence != 0 || strength != 0 || charisma != 0 || cleverness != 0 || battle != 0 || diplomacy != 0)
            {
                character.ConfigureStats(
                    attack,
                    defense,
                    intelligence,
                    strength,
                    charisma,
                    cleverness,
                    battle,
                    diplomacy
                );
            }

            list.Add(character);
        }
    }

    private static int ParseInt(string value)
    {
        return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result)
            ? result
            : 0;
    }

    private static Sex ParseSex(string value)
    {
        if (TryParseEnum(value, out Sex parsed))
        {
            return parsed;
        }

        return Sex.Unknown;
    }

    private static CharacterState ParseState(string value)
    {
        if (TryParseEnum(value, out CharacterState parsed))
        {
            return parsed;
        }

        return CharacterState.Unknown;
    }

    private static PersonalityType ParsePersonality(string value)
    {
        if (TryParseEnum(value, out PersonalityType parsed))
        {
            return parsed;
        }

        return PersonalityType.Unknown;
    }

    private static bool TryParseEnum<TEnum>(string value, out TEnum parsed)
        where TEnum : struct, Enum
    {
        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var numeric)
            && Enum.IsDefined(typeof(TEnum), numeric))
        {
            parsed = (TEnum)Enum.ToObject(typeof(TEnum), numeric);
            return true;
        }

        if (Enum.TryParse(value, ignoreCase: true, out parsed))
        {
            return true;
        }

        parsed = default;
        return false;
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

        MapIfPresent(map, "gender", "sex");
        MapIfPresent(map, "status", "state");
        MapIfPresent(map, "loyality", "loyalty");
        MapIfPresent(map, "kunkou", "merits");
        MapIfPresent(map, "faction", "factionid");
        MapIfPresent(map, "haouid", "factionid");
        MapIfPresent(map, "kingid", "factionid");
        MapIfPresent(map, "lordid", "overlordid");
        MapIfPresent(map, "friendship", "friendshipintimacy");
        MapIfPresent(map, "intimacy", "friendshipintimacy");
        MapIfPresent(map, "temper", "personality");

        MapIfPresent(map, "father", "fatherid");
        MapIfPresent(map, "mother", "motherid");
        MapIfPresent(map, "spouse", "partnerid");
        MapIfPresent(map, "husband", "partnerid");
        MapIfPresent(map, "wife", "partnerid");

        MapIfPresent(map, "class", "rank");

        MapIfPresent(map, "negotiation", "diplomacy");
        MapIfPresent(map, "melee", "strength");

        MapIfPresent(map, "int", "intelligence");
        MapIfPresent(map, "iq", "intelligence");

        MapIfPresent(map, "fleetattack", "attack");
        MapIfPresent(map, "fleetcommandattack", "attack");
        MapIfPresent(map, "fleetdefense", "defense");
        MapIfPresent(map, "fleetcommanddefense", "defense");

        return map;
    }

    private static Dictionary<string, int> BuildDefaultMap()
    {
        return new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["id"] = 0,
            ["name"] = 1,
            ["sex"] = 2,
            ["state"] = 3,
            ["age"] = 4,
            ["loyalty"] = 5,
            ["merits"] = 6,
            ["factionid"] = 7,
            ["overlordid"] = 8,
            ["friendshipintimacy"] = 9,
            ["personality"] = 10
        };
    }

    private static bool HasRequiredKeys(Dictionary<string, int> map)
    {
        return map.ContainsKey("id") && map.ContainsKey("name");
    }

    private static int GetInt(string[] row, Dictionary<string, int> map, string key, int defaultValue)
    {
        if (!map.TryGetValue(key, out var index))
        {
            return defaultValue;
        }

        var value = GetCell(row, index);
        return string.IsNullOrWhiteSpace(value) ? defaultValue : ParseInt(value);
    }

    private static string GetString(string[] row, Dictionary<string, int> map, string key, string defaultValue)
    {
        if (!map.TryGetValue(key, out var index))
        {
            return defaultValue;
        }

        var value = GetCell(row, index);
        return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
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

    private static void MapIfPresent(Dictionary<string, int> map, string alias, string target)
    {
        if (map.ContainsKey(target))
        {
            return;
        }

        if (map.TryGetValue(alias, out var index))
        {
            map[target] = index;
        }
    }
}
