namespace Spacer.Application.Services;

using Spacer.Application.Ports;
using Spacer.Domain.Entities;
using Spacer.Domain.Services;
using Spacer.Domain.ValueObjects;
using Spacer.Domain.Enums;
using System.Collections.Generic;

public sealed class FactionPoliticsTurnService
{
    private readonly ICharacterRoster _roster;
    private readonly FactionPoliticsService _politicsService;
    private readonly IRandomSource _random;

    public FactionPoliticsTurnService( ICharacterRoster roster, FactionPoliticsService politicsService, IRandomSource random )
    {
        _roster = roster;
        _politicsService = politicsService;
        _random = random;
    }

    public void Apply(GameRules gameRules, FactionPoliticsRules rules)
    {
        var characters = _roster.GetAll();
        if (characters.Count == 0)
        {
            return;
        }

        var leaders = new List<Character>();
        foreach (var character in characters)
        {
            if (character.State != CharacterState.Active)
            {
                continue;
            }

            if (character.FactionId == character.Id)
            {
                leaders.Add(character);
            }
        }

        foreach (var candidate in characters)
        {
            if (candidate.Id == gameRules.PlayerOverlordId)
            {
                continue;
            }

            _politicsService.TryJoinFaction( candidate, leaders, rules, _random, gameRules.PlayerOverlordId );
        }
    }
}
