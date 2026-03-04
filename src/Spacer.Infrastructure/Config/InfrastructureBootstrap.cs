namespace Spacer.Infrastructure.Config;

using System.Collections.Generic;
using System.IO;
using Spacer.Application.Config;
using Spacer.Application.Ports;
using Spacer.Application.Services;
using Spacer.Domain.Services;
using Spacer.Infrastructure.Persistence;
using Spacer.Infrastructure.Services;
using Spacer.Application.Events;

public static class InfrastructureBootstrap
{
    public static InfrastructureServices Create(GameConfig config, string dataRoot)
    {
        var scenario = 1; // TODO: make this configurable from menu on start and support multiple scenarios
        var characterPaths = new List<string>();
        var shipSpecPath = Path.Combine(dataRoot, "ship_specs.csv");
        var weaponIdPath = Path.Combine(dataRoot, "weapon_ids.csv");
        var weaponSpecPath = Path.Combine(dataRoot, "weapon_specs.csv");
        var factionPath = Path.Combine(dataRoot, "factions.csv");
        var itemPath = Path.Combine(dataRoot, "items.csv");
        var namePoolPath = Path.Combine(dataRoot, "name_pool.csv");
        var eventPath = Path.Combine(dataRoot, "events");
        if (!File.Exists(itemPath))
        {
            itemPath = Path.Combine(dataRoot, "item.csv");
        }
        var characterBasePath = Path.Combine(dataRoot, "characters_base.csv");
        if (File.Exists(characterBasePath))
        {
            characterPaths.Add(characterBasePath);
        }
        var scenarioCharactersPath = Path.Combine(dataRoot, "characters_scenario_" + scenario + ".csv");
        if (File.Exists(scenarioCharactersPath))
        {
            characterPaths.Add(scenarioCharactersPath);
        }

        var mainMapPath = Path.Combine(dataRoot, "main_map_" + scenario + ".csv");
        var defMapPath = Path.Combine(dataRoot, "defmap_" + scenario + ".csv");
        var planetPath = Path.Combine(dataRoot, "planets_scenario_" + scenario + ".csv");

        var shipCatalog = new CsvShipSpecCatalog(shipSpecPath);
        var weaponResolver = new CsvWeaponIdResolver(weaponIdPath);
        var factionCatalog = new CsvFactionCatalog(factionPath);
        var weaponNameFormatter = new WeaponNameFormatter(factionCatalog);
        var weaponCatalog = new CsvWeaponSpecCatalog(weaponSpecPath, weaponNameFormatter);
        var itemCatalog = new CsvItemCatalog(itemPath);
        var namePool = File.Exists(namePoolPath)
            ? new CsvNamePool(namePoolPath)
            : new CsvNamePool(Path.Combine(dataRoot, "names.csv"));
        var eventCatalog = new JsonEventCatalog(eventPath);
        var eventStateStore = new InMemoryEventStateStore();
        var eventQueue = new InMemoryEventQueue();
        var eventRenderer = new EventRenderer();
        var eventPreconditionEvaluator = new EventPreconditionEvaluator();
        var eventDispatcher = new EventDispatcher(eventCatalog, eventRenderer, eventPreconditionEvaluator);
        var mapLayoutRepository = new CsvMapLayoutRepository(mainMapPath, defMapPath);
        var planetRepository = new CsvPlanetRepository(planetPath);
        var specStore = new InMemoryPlanetFleetSpecStore();
        var planetResearch = new PlanetResearchService();
        var fleetPostureProvider = new StubFleetPostureProvider();
        var characterRepository = new CsvCharacterRepository( characterPaths, config.FactionPoliticsRules );
        var characterRoster = characterRepository;
        var gameClock = new GameClock(startYear: 1, startMonth: config.GameRules.CurrentMonth, monthsInYear: config.GameRules.MonthsInYear);
        var eventContextBuilder = new EventContextBuilder(characterRepository, gameClock, () => config.GameRules.PlayerOverlordId);

        var rebuildService = new FleetSpecRebuildService(
            shipCatalog,
            weaponResolver,
            specStore,
            planetResearch,
            config
        );

        var fleetSpecRebuilder = new FleetSpecRebuilder(rebuildService);
        var judgementLookup = new StubJudgementLookup(config);

        return new InfrastructureServices(
            judgementLookup,
            fleetSpecRebuilder,
            shipCatalog,
            weaponResolver,
            weaponCatalog,
            factionCatalog,
            itemCatalog,
            namePool,
            eventCatalog,
            eventStateStore,
            eventQueue,
            eventDispatcher,
            eventContextBuilder,
            gameClock,
            mapLayoutRepository,
            specStore,
            fleetPostureProvider,
            characterRepository,
            characterRoster,
            planetRepository,
            characterRepository
        );
    }
}

public sealed record InfrastructureServices(
    IJudgementLookup JudgementLookup,
    IFleetSpecRebuilder FleetSpecRebuilder,
    IShipSpecCatalog ShipSpecCatalog,
    IWeaponIdResolver WeaponIdResolver,
    IWeaponSpecCatalog WeaponSpecCatalog,
    IFactionCatalog FactionCatalog,
    IItemCatalog ItemCatalog,
    INamePool NamePool,
    IEventCatalog EventCatalog,
    IEventStateStore EventStateStore,
    IEventQueue EventQueue,
    EventDispatcher EventDispatcher,
    EventContextBuilder EventContextBuilder,
    IGameTime GameTime,
    IMapLayoutRepository MapLayoutRepository,
    IPlanetFleetSpecStore PlanetFleetSpecStore,
    IFleetPostureProvider FleetPostureProvider,
    ICharacterRepository CharacterRepository,
    ICharacterRoster CharacterRoster,
    IPlanetRepository PlanetRepository,
    ICharacterStore CharacterStore
);
