namespace Spacer.Domain.Entities;

using System;
using Spacer.Domain.ValueObjects;

public sealed class Fleet
{
    private Fleet(EntityId id, Position position)
    {
        Id = id;
        Position = position;
    }

    public static Fleet Create(EntityId id, Position position) => new(id, position);
    public static Fleet Create(int id, Position position) => new(EntityId.Create(id), position);

    public EntityId Id { get; }
    public EntityId OverlordId { get; private set; } = EntityId.None;

    public int State { get; private set; }
    public bool IsFortress { get; private set; }

    public Position Position { get; private set; }
    public Position Destination { get; private set; }

    public int NumberOfShips { get; private set; }

    public EntityId CommanderId1 { get; private set; } = EntityId.None;
    public EntityId CommanderId2 { get; private set; } = EntityId.None;
    public EntityId CommanderId3 { get; private set; } = EntityId.None;
    public EntityId CommanderId4 { get; private set; } = EntityId.None;

    public int Attack { get; private set; }
    public int AntiAir { get; private set; }
    public int Defense { get; private set; }
    public int Maneuvering { get; private set; }
    public int Minesweeping { get; private set; }
    public int Construction { get; private set; }

    public int LandingPodLoading { get; private set; }
    public int CarrierLoading { get; private set; }
    public int ShipLoading { get; private set; }

    public void SetDestination(Position destination)
    {
        Destination = destination;
    }
}
