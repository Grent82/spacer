namespace Spacer.Application.UseCases;

using System;
using System.Collections.Generic;
using Spacer.Application.Config;
using Spacer.Application.Events;
using Spacer.Application.Ports;
using Spacer.Application.Services;
using Spacer.Domain.Entities;
using Spacer.Domain.ValueObjects;

public sealed class RunTurnUseCase
{
    private readonly IPlanetRepository _planets;
    private readonly ICharacterRepository _characters;
    private readonly IFleetPostureProvider _fleetPostureProvider;
    private readonly EconomyTurnService _economyTurnService;
    private readonly WeaponResearchProgressionService _researchService;
    private readonly FactionPoliticsTurnService _factionPoliticsService;
    private readonly CharacterLifecycleTurnService _characterLifecycleService;
    private readonly IEventQueue _eventQueue;
    private readonly EventDispatcher _eventDispatcher;
    private readonly EventContextBuilder _eventContextBuilder;
    private readonly IGameTime _gameTime;
    private readonly GameConfig _config;

    public RunTurnUseCase(
        IPlanetRepository planets,
        ICharacterRepository characters,
        IFleetPostureProvider fleetPostureProvider,
        EconomyTurnService economyTurnService,
        WeaponResearchProgressionService researchService,
        FactionPoliticsTurnService factionPoliticsService,
        CharacterLifecycleTurnService characterLifecycleService,
        IEventQueue eventQueue,
        EventDispatcher eventDispatcher,
        EventContextBuilder eventContextBuilder,
        IGameTime gameTime,
        GameConfig config
    )
    {
        _planets = planets;
        _characters = characters;
        _fleetPostureProvider = fleetPostureProvider;
        _economyTurnService = economyTurnService;
        _researchService = researchService;
        _factionPoliticsService = factionPoliticsService;
        _characterLifecycleService = characterLifecycleService;
        _eventQueue = eventQueue;
        _eventDispatcher = eventDispatcher;
        _eventContextBuilder = eventContextBuilder;
        _gameTime = gameTime;
        _config = config;
    }

    public IReadOnlyList<DispatchedEvent> Execute()
    {
        var rules = _config.GameRules with { CurrentMonth = _gameTime.Month };

        // 1) Reset per-turn flags.
        foreach (var planet in _planets.GetAll())
        {
            planet.ResetTurnFlags();
        }

        // 2) Economy tick (production, research, salary income, opinion drift, loyalty impact, population growth).
        var allPlanets = _planets.GetAll();
        _economyTurnService.ApplyPlanetEconomy(
            allPlanets,
            rules,
            _config.PlanetEconomyRules,
            _config.PlanetResearchRules,
            rulerId => rulerId.IsNone
                ? FleetPostureSummary.Empty
                : _fleetPostureProvider.GetForRuler(rulerId)
        );

        // 3) Research progression (weapon release unlocks).
        foreach (var planet in allPlanets)
        {
            if (planet.RulerId.IsNone)
            {
                continue;
            }

            var ruler = _characters.FindById(planet.RulerId);
            if (ruler is null)
            {
                continue;
            }

            _researchService.TryUnlockWeaponRelease(planet, ruler.Personality);
        }

        // 4) Character lifecycle (aging, births, deaths).
        var lifecycleResult = _characterLifecycleService.Apply(
            rules,
            _config.CharacterLifecycleRules,
            _config.FactionPoliticsRules
        );

        // 5) Faction politics (defection/joining).
        _factionPoliticsService.Apply(rules, _config.FactionPoliticsRules, _config.PlanetEconomyRules);

        // 6) Enqueue data-only events and render them for the UI.
        EnqueueBirthEvents(lifecycleResult, rules);

        var queued = _eventQueue.Drain();
        if (queued.Count == 0)
        {
            _gameTime.AdvanceTurn();
            return Array.Empty<DispatchedEvent>();
        }

        var rendered = new List<DispatchedEvent>(queued.Count);
        foreach (var request in queued)
        {
            var result = _eventDispatcher.RunById(request.EventId, request.Context);
            if (result is null)
            {
                continue;
            }

            rendered.Add(new DispatchedEvent(request.EventId, result));
        }

        _gameTime.AdvanceTurn();
        return rendered;
    }

    private void EnqueueBirthEvents(CharacterLifecycleResult lifecycleResult, GameRules rules)
    {
        if (lifecycleResult.Births.Count == 0)
        {
            return;
        }

        if (rules.PlayerOverlordId.IsNone)
        {
            return;
        }

        var player = _characters.FindById(rules.PlayerOverlordId);
        foreach (var newborn in lifecycleResult.Births)
        {
            if (newborn.FatherId != rules.PlayerOverlordId && newborn.MotherId != rules.PlayerOverlordId)
            {
                continue;
            }

            var mother = _characters.FindById(newborn.MotherId);
            var father = _characters.FindById(newborn.FatherId);

            _eventQueue.Enqueue(new EventRequest(
                "birth_olivia_child",
                _eventContextBuilder.Build(
                    new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["birthFlag"] = "true",
                        ["childName"] = newborn.Name
                    },
                    ("mother", mother),
                    ("father", father),
                    ("child", newborn)
                )
            ));
        }
    }
}
