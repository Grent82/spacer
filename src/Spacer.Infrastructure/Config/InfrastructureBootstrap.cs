namespace Spacer.Infrastructure.Config;

using System.Collections.Generic;
using System.IO;
using Spacer.Application.Config;
using Spacer.Application.Ports;
using Spacer.Application.Services;
using Spacer.Domain.Services;
using Spacer.Infrastructure.Persistence;
using Spacer.Infrastructure.Services;

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

        var shipCatalog = new CsvShipSpecCatalog(shipSpecPath);
        var weaponResolver = new CsvWeaponIdResolver(weaponIdPath);
        var factionCatalog = new CsvFactionCatalog(factionPath);
        var weaponNameFormatter = new WeaponNameFormatter(factionCatalog);
        var weaponCatalog = new CsvWeaponSpecCatalog(weaponSpecPath, weaponNameFormatter);
        var itemCatalog = new CsvItemCatalog(itemPath);
        var specStore = new InMemoryPlanetFleetSpecStore();
        var planetResearch = new PlanetResearchService();
        var fleetPostureProvider = new StubFleetPostureProvider();
        var characterRepository = new CsvCharacterRepository( characterPaths, config.FactionPoliticsRules );
        var characterRoster = characterRepository;

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
            specStore,
            fleetPostureProvider,
            characterRepository,
            characterRoster
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
    IPlanetFleetSpecStore PlanetFleetSpecStore,
    IFleetPostureProvider FleetPostureProvider,
    ICharacterRepository CharacterRepository,
    ICharacterRoster CharacterRoster
);
