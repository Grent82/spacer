namespace Spacer.Domain.Entities;

using System;
using Spacer.Domain.Enums;
using Spacer.Domain.ValueObjects;

public sealed class Planet
{
    private Planet(EntityId id, string name, Position position)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name must be provided.", nameof(name));
        }

        Id = id;
        Name = name;
        Position = position;
    }

    public static Planet Create(EntityId id, string name, Position position) => new(id, name, position);
    public static Planet Create(int id, string name, Position position) => new(EntityId.Create(id), name, position);

    public EntityId Id { get; }
    public string Name { get; private set; }

    public PlanetType Type { get; private set; }
    public bool IsCapitalCity { get; private set; }

    public EntityId RulerId { get; private set; } = EntityId.None;
    public EntityId PlayerOwnerId { get; private set; } = EntityId.None;
    public EntityId FeudalLordId { get; private set; } = EntityId.None;

    public Position Position { get; private set; }

    public int LandBattleNumber { get; private set; }
    public int GroundAttack { get; private set; }
    public int GroundDefense { get; private set; }

    public int CitizensLoyalty { get; private set; }
    public int InformationLeak { get; private set; }
    public int Population { get; private set; }

    public int Production { get; private set; }
    public Money Gold { get; private set; } = Money.Zero;
    public int PublicOpinion { get; private set; }

    public EntityId AttackTargetId { get; private set; } = EntityId.None;
    public bool DidEspionage { get; private set; }

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name must be provided.", nameof(name));
        }

        Name = name;
    }
}
