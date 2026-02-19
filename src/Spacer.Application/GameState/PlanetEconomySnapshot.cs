namespace Spacer.Application.GameState;

using Spacer.Domain.Entities;
using Spacer.Domain.ValueObjects;

public readonly record struct PlanetEconomySnapshot( EntityId PlanetId, int Population, int Production, int PublicOpinion, int CitizensLoyalty, Money Gold )
{
    public static PlanetEconomySnapshot FromPlanet(Planet planet)
    {
        return new PlanetEconomySnapshot(
            planet.Id,
            planet.Population,
            planet.Production,
            planet.PublicOpinion,
            planet.CitizensLoyalty,
            planet.Gold
        );
    }
}
