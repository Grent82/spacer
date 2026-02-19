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
    public int SystemId { get; private set; }

    public EntityId RulerId { get; private set; } = EntityId.None;
    public EntityId PlayerOwnerId { get; private set; } = EntityId.None;
    public EntityId FeudalLordId { get; private set; } = EntityId.None;
    public EntityId OwnerFactionId { get; private set; } = EntityId.None;

    public Position Position { get; private set; }

    public int LandBattleNumber { get; private set; }
    public int GroundAttack { get; private set; }
    public int GroundDefense { get; private set; }

    public int CitizensLoyalty { get; private set; } = 50;
    public int InformationLeak { get; private set; }
    public int Population { get; private set; }

    public int Production { get; private set; }
    public Money Gold { get; private set; } = Money.Zero;
    public int PublicOpinion { get; private set; }

    public EntityId AttackTargetId { get; private set; } = EntityId.None;
    public bool DidEspionage { get; private set; }

    public PlanetResearch Research { get; } = new();

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name must be provided.", nameof(name));
        }

        Name = name;
    }

    public void SetSystemId(int systemId)
    {
        SystemId = systemId < 0 ? 0 : systemId;
    }

    public void SetRuler(EntityId rulerId, EntityId rulerFactionId)
    {
        RulerId = rulerId;

        if (rulerId.IsNone)
        {
            OwnerFactionId = EntityId.None;
            return;
        }

        if (rulerFactionId.IsNone)
        {
            throw new ArgumentException("Ruler faction must be provided when a ruler is set.", nameof(rulerFactionId));
        }

        OwnerFactionId = rulerFactionId;
    }

    public void SetRuler(int rulerId, int rulerFactionId)
    {
        SetRuler(EntityId.Create(rulerId), EntityId.Create(rulerFactionId));
    }

    public void AdjustPopulation(int delta, PlanetEconomyRules rules)
    {
        var next = Population + delta;
        if (next < rules.MinPopulation)
        {
            next = rules.MinPopulation;
        }
        if (next > rules.MaxPopulation)
        {
            next = rules.MaxPopulation;
        }
        Population = next;
    }

    public void AdjustPublicOpinion(int delta, PlanetEconomyRules rules)
    {
        var next = PublicOpinion + delta;
        if (next < rules.MinPublicOpinion)
        {
            next = rules.MinPublicOpinion;
        }
        if (next > rules.MaxPublicOpinion)
        {
            next = rules.MaxPublicOpinion;
        }
        PublicOpinion = next;
    }

    public void AdjustCitizensLoyalty(int delta, PlanetEconomyRules rules)
    {
        var next = CitizensLoyalty + delta;
        if (next < rules.MinLoyalty)
        {
            next = rules.MinLoyalty;
        }
        if (next > rules.MaxLoyalty)
        {
            next = rules.MaxLoyalty;
        }
        CitizensLoyalty = next;
    }

    public void AdjustGold(int delta, PlanetEconomyRules rules)
    {
        var next = Gold.Amount + delta;
        if (next < 0)
        {
            next = 0;
        }
        if (next > rules.MaxFunds)
        {
            next = rules.MaxFunds;
        }
        Gold = new Money(next);
    }
}
