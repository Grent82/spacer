namespace Spacer.Domain.ValueObjects;

using Spacer.Domain.Enums;

public readonly record struct MarriageRules(
    int MinMarriageAge,
    bool RequireSameFactionWhenActive,
    bool AllowCommonerMarriage,
    bool AllowEnslavedMarriage
)
{
    public static readonly MarriageRules Default = new(
        MinMarriageAge: 15,
        RequireSameFactionWhenActive: true,
        AllowCommonerMarriage: true,
        AllowEnslavedMarriage: false
    );

    public bool IsStateEligible(CharacterState state)
    {
        return state switch
        {
            CharacterState.Active => true,
            CharacterState.Training => true,
            CharacterState.Commoner => AllowCommonerMarriage,
            CharacterState.Enslaved => AllowEnslavedMarriage,
            _ => false
        };
    }
}
