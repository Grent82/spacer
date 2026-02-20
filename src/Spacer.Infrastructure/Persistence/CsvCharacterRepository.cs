namespace Spacer.Infrastructure.Persistence;

using System;
using System.Collections.Generic;
using System.Globalization;
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

    private static bool IsHeader(string value)
    {
        return string.Equals(value.Trim(), "id", StringComparison.OrdinalIgnoreCase);
    }

    private static void LoadCharacters( string path, List<Character> list, HashSet<int> knownIds, FactionPoliticsRules rules) {
        foreach (var row in CsvRowReader.ReadRows(path))
        {
            if (row.Length == 0)
            {
                continue;
            }

            if (IsHeader(row[0]))
            {
                continue;
            }

            var id = ParseInt(GetCell(row, 0));
            if (id <= 0)
            {
                continue;
            }

            if (!knownIds.Add(id))
            {
                continue;
            }

            var name = GetCell(row, 1);
            if (string.IsNullOrWhiteSpace(name))
            {
                name = $"Character {id}";
            }

            var sex = ParseSex(GetCell(row, 2));
            var character = Character.Create(id, name, sex);

            character.SetState(ParseState(GetCell(row, 3)));
            character.SetAge(ParseInt(GetCell(row, 4)));
            character.SetLoyalty(ParseInt(GetCell(row, 5)), rules);
            character.SetMerits(ParseInt(GetCell(row, 6)));

            var factionId = ParseInt(GetCell(row, 7));
            var overlordId = ParseInt(GetCell(row, 8));
            if (factionId > 0 || overlordId > 0)
            {
                var factionEntityId = factionId > 0 ? EntityId.Create(factionId) : EntityId.None;
                var overlordEntityId = overlordId > 0 ? EntityId.Create(overlordId) : EntityId.None;
                character.SetFaction(factionEntityId, overlordEntityId);
            }

            character.SetFriendshipIntimacy(ParseInt(GetCell(row, 9)));
            character.SetPersonality(ParsePersonality(GetCell(row, 10)));

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
}
