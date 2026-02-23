namespace Spacer.Domain.Services;

using Spacer.Domain.Entities;
using Spacer.Domain.Enums;
using Spacer.Domain.ValueObjects;

public sealed class MarriageService
{
    public bool IsMarriageEligible(Character candidate, MarriageRules rules)
    {
        if (candidate.Age < rules.MinMarriageAge)
        {
            return false;
        }

        return rules.IsStateEligible(candidate.State);
    }

    public bool IsMarriageValid(Character left, Character right, MarriageRules rules)
    {
        if (left.Id == right.Id)
        {
            return false;
        }

        if (!IsMarriageEligible(left, rules) || !IsMarriageEligible(right, rules))
        {
            return false;
        }

        if (rules.RequireSameFactionWhenActive
            && left.State == CharacterState.Active
            && right.State == CharacterState.Active
            && left.FactionId != right.FactionId)
        {
            return false;
        }

        return true;
    }

    public bool IsSpouseStatusInvalid(Character spouse, Character partner, MarriageRules rules)
    {
        if (spouse.State == CharacterState.Active
            && spouse.FactionId != partner.FactionId)
        {
            return true;
        }

        if (!rules.IsStateEligible(spouse.State))
        {
            return true;
        }

        return false;
    }

    public bool TryMarry(Character left, Character right, MarriageRules rules)
    {
        if (!IsMarriageValid(left, right, rules))
        {
            return false;
        }

        if (!left.PartnerId.IsNone || !right.PartnerId.IsNone)
        {
            return false;
        }

        left.SetPartner(right.Id);
        right.SetPartner(left.Id);
        return true;
    }

    public void Divorce(Character left, Character right)
    {
        if (left.PartnerId == right.Id)
        {
            left.ClearPartner();
        }

        if (right.PartnerId == left.Id)
        {
            right.ClearPartner();
        }
    }
}
