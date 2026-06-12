namespace Spacer.Infrastructure.Events;

using System;
using System.Text.Json;
using Spacer.Application.Events;
using Spacer.Application.Ports;
using Spacer.Domain.Entities;
using Spacer.Domain.Services;
using Spacer.Domain.ValueObjects;

/// <summary>
/// Handles the StartPregnancy action - initiates pregnancy on a target character.
/// Used for event-driven conception (e.g., marriage events, special story events).
/// </summary>
public sealed class StartPregnancyActionHandler : IEventActionHandler
{
    private readonly ICharacterStore _characters;
    private readonly ConceptionService _conceptionService;

    public StartPregnancyActionHandler(ICharacterStore characters, ConceptionService conceptionService)
    {
        _characters = characters;
        _conceptionService = conceptionService;
    }

    public string Action => "StartPregnancy";

    public void Execute(EventStep step, EventContext context)
    {
        if (step.Args is null)
        {
            return;
        }

        // Get target character (mother).
        var targetId = GetEntityIdFromArgs(step.Args, "targetId", context.TargetId);
        var target = _characters.FindById(targetId);
        if (target is null || target.Sex != Domain.Enums.Sex.Female)
        {
            return;
        }

        // Get actor character (father) - optional.
        var actorId = GetEntityIdFromArgs(step.Args, "actorId", context.ActorId);
        var actor = actorId.IsNone ? null : _characters.FindById(actorId);

        // Get partner ID to use (from actor or explicit partnerId arg).
        var partnerId = TryGetPartnerId(step.Args, actor);

        // Start pregnancy on target.
        target.StartPregnancy(partnerId);
    }

    /// <summary>
    /// Tries to get partner ID from args or actor.
    /// </summary>
    private EntityId TryGetPartnerId(Dictionary<string, JsonElement> args, Character? actor)
    {
        // Check for explicit partnerId in args.
        if (args.TryGetValue("partnerId", out var partnerIdElement))
        {
            if (partnerIdElement.ValueKind == JsonValueKind.Number)
            {
                if (partnerIdElement.TryGetInt32(out var partnerIdValue))
                {
                    return EntityId.Create(partnerIdValue);
                }
            }
        }

        // Use actor as partner if available.
        if (actor is not null)
        {
            return actor.Id;
        }

        return EntityId.None;
    }

    /// <summary>
    /// Gets EntityId from args or falls back to context value.
    /// </summary>
    private static EntityId GetEntityIdFromArgs(
        Dictionary<string, JsonElement> args,
        string key,
        EntityId contextDefault
    )
    {
        if (args.TryGetValue(key, out var element))
        {
            if (element.ValueKind == JsonValueKind.Number)
            {
                if (element.TryGetInt32(out var value))
                {
                    return EntityId.Create(value);
                }
            }
        }

        return contextDefault;
    }
}

/// <summary>
/// Handles the CheckConception action - attempts conception and sets result flags.
/// </summary>
public sealed class CheckConceptionActionHandler : IEventActionHandler
{
    private readonly ICharacterStore _characters;
    private readonly ConceptionService _conceptionService;
    private readonly IEventStateStore _stateStore;

    public CheckConceptionActionHandler(
        ICharacterStore characters,
        ConceptionService conceptionService,
        IEventStateStore stateStore
    )
    {
        _characters = characters;
        _conceptionService = conceptionService;
        _stateStore = stateStore;
    }

    public string Action => "CheckConception";

    public void Execute(EventStep step, EventContext context)
    {
        if (step.Args is null)
        {
            return;
        }

        // Get target character (female).
        var targetId = GetEntityIdFromArgs(step.Args, "targetId", context.TargetId);
        var target = _characters.FindById(targetId);
        if (target is null || target.Sex != Domain.Enums.Sex.Female)
        {
            _stateStore.SetFlag("conception_success", false);
            _stateStore.SetFlag("conception_failed", true);
            return;
        }

        // Get partner (actor).
        var actorId = GetEntityIdFromArgs(step.Args, "actorId", context.ActorId);
        var actor = actorId.IsNone ? null : _characters.FindById(actorId);

        // Use default pregnancy rules for event-based conception.
        var rules = PregnancyRules.Default with
        {
            ConceptionChancePercent = 50, // Higher chance for event-driven conception
            ConceptionWithPartnerOnly = false
        };

        var result = _conceptionService.TryConceive(target, actor, rules);

        // Set result flags.
        _stateStore.SetFlag("conception_success", result.Success);
        _stateStore.SetFlag("conception_failed", result.Failed);
        _stateStore.SetFlag("conception_infertile", result.Infertile);
        _stateStore.SetFlag("conception_no_partner", result.NoPartner);
        _stateStore.SetFlag("conception_blocked", result.BlockedBySpecialFlag);

        // If successful, start pregnancy.
        if (result.Success)
        {
            var partnerId = GetPartnerId(step.Args, actor);
            target.StartPregnancy(partnerId);
        }
    }

    private EntityId GetPartnerId(Dictionary<string, JsonElement> args, Character? actor)
    {
        if (args.TryGetValue("partnerId", out var partnerIdElement))
        {
            if (partnerIdElement.ValueKind == JsonValueKind.Number)
            {
                if (partnerIdElement.TryGetInt32(out var partnerIdValue))
                {
                    return EntityId.Create(partnerIdValue);
                }
            }
        }

        if (actor is not null)
        {
            return actor.Id;
        }

        return EntityId.None;
    }

    private static EntityId GetEntityIdFromArgs(
        Dictionary<string, JsonElement> args,
        string key,
        EntityId contextDefault
    )
    {
        if (args.TryGetValue(key, out var element))
        {
            if (element.ValueKind == JsonValueKind.Number)
            {
                if (element.TryGetInt32(out var value))
                {
                    return EntityId.Create(value);
                }
            }
        }

        return contextDefault;
    }
}
