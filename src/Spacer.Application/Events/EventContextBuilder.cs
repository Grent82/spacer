namespace Spacer.Application.Events;

using System;
using System.Collections.Generic;
using Spacer.Application.Ports;
using Spacer.Domain.Entities;
using Spacer.Domain.ValueObjects;

public sealed class EventContextBuilder
{
    private readonly ICharacterRepository _characters;
    private readonly IGameTime _gameTime;
    private readonly Func<EntityId> _playerIdProvider;

    public EventContextBuilder(ICharacterRepository characters, IGameTime gameTime, Func<EntityId> playerIdProvider)
    {
        _characters = characters;
        _gameTime = gameTime;
        _playerIdProvider = playerIdProvider;
    }

    public EventRenderContext Build(
        IReadOnlyDictionary<string, string>? extra = null,
        params (string key, Character? character)[] characters
    )
    {
        var variables = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        variables["year"] = _gameTime.Year.ToString();
        variables["month"] = _gameTime.Month.ToString();

        if (_gameTime.MonthsInYear > 0)
        {
            variables["monthsInYear"] = _gameTime.MonthsInYear.ToString();
        }

        var playerId = _playerIdProvider();
        if (!playerId.IsNone)
        {
            var player = _characters.FindById(playerId);
            AddCharacterVariables(variables, "player", player, addNameAlias: true);
        }

        if (characters is { Length: > 0 })
        {
            foreach (var entry in characters)
            {
                AddCharacterVariables(variables, entry.key, entry.character, addNameAlias: true);
            }
        }

        if (extra is not null)
        {
            foreach (var pair in extra)
            {
                variables[pair.Key] = pair.Value;
            }
        }

        return new EventRenderContext(variables);
    }

    private static void AddCharacterVariables(
        IDictionary<string, string> variables,
        string key,
        Character? character,
        bool addNameAlias
    )
    {
        if (character is null || string.IsNullOrWhiteSpace(key))
        {
            return;
        }

        if (addNameAlias)
        {
            variables[key] = character.Name;
        }

        variables[$"{key}Name"] = character.Name;
        variables[$"{key}Id"] = character.Id.Value.ToString();
        variables[$"{key}State"] = ((int)character.State).ToString();
        variables[$"{key}Status0"] = ((int)character.State).ToString();
        variables[$"{key}Merits"] = character.Merits.ToString();
        variables[$"{key}Rank"] = character.Rank.ToString();
        variables[$"{key}FactionId"] = character.FactionId.Value.ToString();
        variables[$"{key}OverlordId"] = character.OverlordId.Value.ToString();
    }
}
