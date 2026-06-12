namespace Spacer.Domain.Services;

using Spacer.Domain.Entities;
using Spacer.Domain.ValueObjects;

/// <summary>
/// Handles conception logic for pregnancy mechanics.
/// </summary>
public sealed class ConceptionService
{
    private readonly IRandomSource _random;

    public ConceptionService(IRandomSource random)
    {
        _random = random;
    }

    /// <summary>
    /// Attempts conception for a female character with a partner.
    /// </summary>
    /// <param name="female">The female character attempting conception.</param>
    /// <param name="male">The male partner (optional, may be null for random conception).</param>
    /// <param name="rules">Conception rules to apply.</param>
    /// <returns>ConceptionResult indicating success or failure reason.</returns>
    public ConceptionResult TryConceive(
        Character female,
        Character? male,
        PregnancyRules rules
    )
    {
        // Check if already pregnant.
        if (female.PregnancyMonths > 0)
        {
            return ConceptionResult.FailedResult;
        }

        // Check special flags that block conception (immortal/special characters).
        if (female.SpecialFlags == 2)
        {
            return ConceptionResult.BlockedResult;
        }

        // Check infertility threshold.
        if (female.Infertility >= rules.ConceptionInfertilityThreshold)
        {
            return ConceptionResult.InfertileResult;
        }

        // Check partner requirement.
        if (rules.ConceptionWithPartnerOnly)
        {
            if (female.PregnancyPartnerId.IsNone && male is null)
            {
                return ConceptionResult.NoPartnerResult;
            }
        }

        // Roll for conception success.
        var roll = _random.Next(100);
        if (roll < rules.ConceptionChancePercent)
        {
            return ConceptionResult.SuccessResult;
        }

        return ConceptionResult.FailedResult;
    }

    /// <summary>
    /// Calculates modified conception chance based on partner stats.
    /// </summary>
    /// <param name="female">The female character.</param>
    /// <param name="male">The male partner.</param>
    /// <param name="baseChance">Base conception chance percent.</param>
    /// <returns>Modified conception chance percent.</returns>
    public int CalculateModifiedConceptionChance(Character female, Character? male, int baseChance)
    {
        var modifier = 0;

        // Female cleverness bonus (slight increase for higher cleverness).
        if (female.Cleverness > 50)
        {
            modifier += (female.Cleverness - 50) / 10;
        }

        // Male fertility bonus from battle/diplomacy stats (represents overall vitality).
        if (male is not null)
        {
            var maleVitality = (male.Battle + male.Diplomacy) / 2;
            if (maleVitality > 50)
            {
                modifier += (maleVitality - 50) / 20;
            }

            // Male infertility check.
            if (male.Infertility >= 100)
            {
                return 0;
            }
        }

        // Clamp to valid range.
        var modified = baseChance + modifier;
        return Math.Max(1, Math.Min(95, modified));
    }
}
