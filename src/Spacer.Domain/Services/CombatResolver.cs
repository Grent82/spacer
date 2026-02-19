namespace Spacer.Domain.Services;

using Spacer.Domain.Entities;
using Spacer.Domain.ValueObjects;

public sealed class CombatResolver
{
    private readonly CharacterDuelResolver _duelResolver = new();
    private readonly FleetCombatResolver _fleetResolver = new();

    public CharacterDuelResult ResolveCharacterDuel( Character left, Character right, CombatContext context, bool killOnDefeat )
    {
        return _duelResolver.Resolve(left, right, context, killOnDefeat);
    }

    public FleetCombatResult ResolveFleetCombat( FleetCombatant attacker, FleetCombatant defender, CombatContext context )
    {
        return _fleetResolver.Resolve(attacker, defender, context);
    }
}
