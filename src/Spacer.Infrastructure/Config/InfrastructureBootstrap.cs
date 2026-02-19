namespace Spacer.Infrastructure.Config;

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
        var shipSpecPath = Path.Combine(dataRoot, "ship_specs.csv");
        var weaponIdPath = Path.Combine(dataRoot, "weapon_ids.csv");
        var weaponSpecPath = Path.Combine(dataRoot, "weapon_specs.csv");
        var factionPath = Path.Combine(dataRoot, "factions.csv");

        var shipCatalog = new CsvShipSpecCatalog(shipSpecPath);
        var weaponResolver = new CsvWeaponIdResolver(weaponIdPath);
        var factionCatalog = new CsvFactionCatalog(factionPath);
        var weaponNameFormatter = new WeaponNameFormatter(factionCatalog);
        var weaponCatalog = new CsvWeaponSpecCatalog(weaponSpecPath, weaponNameFormatter);
        var specStore = new InMemoryPlanetFleetSpecStore();
        var planetResearch = new PlanetResearchService();

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
            specStore
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
    IPlanetFleetSpecStore PlanetFleetSpecStore
);
