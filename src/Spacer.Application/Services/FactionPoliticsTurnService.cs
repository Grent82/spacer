namespace Spacer.Application.Services;

using Spacer.Application.Ports;
using Spacer.Domain.Entities;
using Spacer.Domain.Services;
using Spacer.Domain.ValueObjects;
using Spacer.Domain.Enums;
using System.Collections.Generic;
using System.Linq;

public sealed class FactionPoliticsTurnService
{
    private readonly ICharacterRoster _roster;
    private readonly IPlanetRepository _planets;
    private readonly FactionPoliticsService _politicsService;
    private readonly IRandomSource _random;

    public FactionPoliticsTurnService( ICharacterRoster roster, IPlanetRepository planets, FactionPoliticsService politicsService, IRandomSource random )
    {
        _roster = roster;
        _planets = planets;
        _politicsService = politicsService;
        _random = random;
    }

    public void Apply(GameRules gameRules, FactionPoliticsRules rules, PlanetEconomyRules economyRules)
    {
        var characters = _roster.GetAll();
        if (characters.Count == 0)
        {
            return;
        }

        var planets = _planets.GetAll();

        // Apply per-turn loyalty drift before other politics.
        ApplyLoyaltyDrift(characters, planets, rules);

        ApplySuccession(characters, rules, economyRules);

        var leaders = GetActiveLeaders(characters);
        if (leaders.Count == 0)
        {
            return;
        }

        foreach (var candidate in characters)
        {
            if (candidate.Id == gameRules.PlayerOverlordId)
            {
                continue;
            }

            if (candidate.State != CharacterState.Active)
            {
                continue;
            }

            var joined = _politicsService.TryJoinFaction(candidate, leaders, rules, _random, gameRules.PlayerOverlordId);
            if (!joined && rules.DefectionCreatesIndependentFaction)
            {
                if (_politicsService.ShouldDefect(candidate, rules, _random))
                {
                    candidate.SetFaction(EntityId.None, EntityId.None);
                }
            }
        }
    }

    private void ApplyLoyaltyDrift(
        IReadOnlyList<Character> characters,
        IReadOnlyList<Planet> planets,
        FactionPoliticsRules rules
    )
    {
        // Group planets by faction for quick lookup.
        var planetByFaction = new Dictionary<EntityId, Planet>();
        foreach (var planet in planets)
        {
            if (!planet.OwnerFactionId.IsNone)
            {
                planetByFaction[planet.OwnerFactionId] = planet;
            }
        }

        // Group characters by faction.
        var factionMembers = new Dictionary<EntityId, List<Character>>();
        foreach (var character in characters)
        {
            if (!character.FactionId.IsNone)
            {
                if (!factionMembers.TryGetValue(character.FactionId, out var list))
                {
                    list = new List<Character>();
                    factionMembers[character.FactionId] = list;
                }
                list.Add(character);
            }
        }

        // Apply drift for each faction.
        foreach (var entry in factionMembers)
        {
            var factionId = entry.Key;
            var members = entry.Value;

            // Get faction's capital planet for public opinion and population.
            planetByFaction.TryGetValue(factionId, out var capital);
            var publicOpinion = capital?.PublicOpinion ?? 50;
            var populationDelta = capital?.Population ?? 0;

            // Simplified: assume peace for now (war detection would need fleet data).
            var isAtWar = false;

            _politicsService.ApplyLoyaltyDrift(members, publicOpinion, populationDelta, isAtWar, rules);
        }
    }

    private void ApplySuccession(
        IReadOnlyList<Character> characters,
        FactionPoliticsRules rules,
        PlanetEconomyRules economyRules
    )
    {
        var deadLeaders = characters
            .Where(character => character.FactionId == character.Id && character.State != CharacterState.Active)
            .ToList();

        if (deadLeaders.Count == 0)
        {
            return;
        }

        var planets = _planets.GetAll();
        foreach (var deadLeader in deadLeaders)
        {
            var members = characters
                .Where(character => character.FactionId == deadLeader.Id && character.State == CharacterState.Active)
                .ToList();

            if (members.Count == 0)
            {
                continue;
            }

            var successor = _politicsService.SelectSuccessor(members, rules);
            if (successor is null)
            {
                foreach (var member in members)
                {
                    member.SetFaction(EntityId.None, EntityId.None);
                }
                continue;
            }

            _politicsService.ApplySuccession(successor, members, rules);
            foreach (var planet in planets)
            {
                if (planet.OwnerFactionId != deadLeader.Id)
                {
                    continue;
                }

                planet.SetOwnerFaction(successor.Id);
                if (rules.SuccessionPlanetLoyaltyPenalty != 0)
                {
                    planet.AdjustCitizensLoyalty(-rules.SuccessionPlanetLoyaltyPenalty, economyRules);
                }
            }
        }
    }

    private static List<Character> GetActiveLeaders(IReadOnlyList<Character> characters)
    {
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

        return leaders;
    }
}
