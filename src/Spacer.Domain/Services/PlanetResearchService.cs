namespace Spacer.Domain.Services;

using Spacer.Domain.Entities;
using Spacer.Domain.ValueObjects;

public sealed class PlanetResearchService
{
    private const int WeaponResearchCategoryCount = 3;

    /// <summary>
    /// Applies research progression based on production investment.
    /// </summary>
    public bool ApplyResearchProgression(
        Planet planet,
        int productionToInvest,
        PlanetResearchRules rules
    )
    {
        if (productionToInvest <= 0)
        {
            return false;
        }

        var progressed = false;

        // Distribute production across all research categories.
        var productionPerCategory = productionToInvest / WeaponResearchCategoryCount;

        for (var i = 0; i < WeaponResearchCategoryCount; i++)
        {
            var currentLevel = planet.Research.GetCurrentLevel(i);
            var growth = ComputeResearchGrowth(productionPerCategory, currentLevel, rules);

            if (growth > 0)
            {
                AdjustResearchLevel(planet, i, growth, rules);
                progressed = true;
            }
        }

        // Update system tech level based on total research.
        UpdateSystemTechLevel(planet, rules);

        return progressed;
    }

    /// <summary>
    /// Computes research growth based on production investment and current level.
    /// </summary>
    private static int ComputeResearchGrowth(int production, int currentLevel, PlanetResearchRules rules)
    {
        if (production <= 0)
        {
            return 0;
        }

        var baseGrowth = production / rules.Progression.ProductionToResearchEfficiency;

        // Growth bonus from existing research (diminishing returns).
        var growthBonus = baseGrowth * rules.Progression.ResearchGrowthBonusPercent * currentLevel / 100000;

        // Apply decay for very high research levels.
        var decay = 0;
        if (currentLevel > 50000)
        {
            decay = baseGrowth * rules.Progression.ResearchDecayPercent / 100;
        }

        var totalGrowth = baseGrowth + growthBonus - decay;
        return Math.Max(0, totalGrowth);
    }

    /// <summary>
    /// Updates system tech level based on total research achievement.
    /// </summary>
    public void UpdateSystemTechLevel(Planet planet, PlanetResearchRules rules)
    {
        var totalResearch = ComputeResearchSum(planet);
        var currentTechLevel = planet.Research.SystemTechLevel;

        // System tech increases every 5000 total research points.
        var targetTechLevel = Math.Min(
            totalResearch / rules.Progression.SystemTechGrowthThreshold,
            rules.MaxSystemTechLevel
        );

        if (targetTechLevel > currentTechLevel)
        {
            SetSystemTechLevel(planet, targetTechLevel, rules);
        }
    }

    /// <summary>
    /// Computes research cost for a given level increase.
    /// </summary>
    public int ComputeResearchCost(int targetLevel, PlanetResearchRules rules)
    {
        if (targetLevel <= 0)
        {
            return 0;
        }

        // Cost is based on target level, not using rules directly.
        return targetLevel * 100;
    }

    public void CopyResearch(Planet source, Planet target)
    {
        target.Research.CopyFrom(source.Research);
    }

    public void AdjustResearchLevel( Planet planet, int index, int delta, PlanetResearchRules rules )
    {
        var current = planet.Research.GetCurrentLevel(index);
        planet.Research.SetCurrentLevel(index, current + delta, rules);
    }

    public void SetResearchLevel( Planet planet, int index, int value, PlanetResearchRules rules )
    {
        planet.Research.SetCurrentLevel(index, value, rules);
    }

    public void SetRemainingWeaponResearch(
        Planet planet,
        int index,
        int value,
        PlanetResearchRules rules
    )
    {
        planet.Research.SetRemainingWeaponResearch(index, value, rules);
    }

    public void SetUpdatedWeaponResearch(Planet planet, int index, int value)
    {
        planet.Research.SetUpdatedWeaponResearch(index, value);
    }

    public void SetUpdatedWeaponResearch(Planet planet, int index, bool value)
    {
        planet.Research.SetUpdatedWeaponResearch(index, value ? 1 : 0);
    }

    public void SetUpdatedResearch(Planet planet, int index, int value)
    {
        planet.Research.SetUpdatedResearch(index, value);
    }

    public void SetUpdatedResearch(Planet planet, int index, bool value)
    {
        planet.Research.SetUpdatedResearch(index, value ? 1 : 0);
    }

    public void SetSystemTechLevel(Planet planet, int value, PlanetResearchRules rules)
    {
        planet.Research.SetSystemTechLevel(value, rules);
    }

    public int ComputeResearchSum(Planet planet)
    {
        return planet.Research.ComputeResearchSum();
    }

    public int ComputeWeaponReleaseStage(int weaponReleaseChanges, int remainingWeaponResearch)
    {
        var stage = weaponReleaseChanges + 1 - remainingWeaponResearch;
        return stage < 1 ? 1 : stage;
    }

    public int GetWeaponReleaseStage(Planet planet, int index, int weaponReleaseChanges)
    {
        var remaining = planet.Research.GetRemainingWeaponResearch(index);
        return ComputeWeaponReleaseStage(weaponReleaseChanges, remaining);
    }

    public void InitializeWeaponReleaseRemaining(
        Planet planet,
        int weaponReleaseChanges,
        PlanetResearchRules rules
    )
    {
        for (var i = 0; i < WeaponResearchCategoryCount; i++)
        {
            planet.Research.SetRemainingWeaponResearch(i, weaponReleaseChanges, rules);
        }
        planet.Research.SetWeaponReleaseProgressSum(0);
    }

    public bool TryMarkWeaponReleaseUpdated(Planet planet, int requiredDelta)
    {
        var currentSum = planet.Research.ComputeResearchSum();
        var delta = currentSum - planet.Research.WeaponReleaseProgressSum;
        if (delta < requiredDelta)
        {
            return false;
        }

        for (var i = 0; i < WeaponResearchCategoryCount; i++)
        {
            planet.Research.SetUpdatedWeaponResearch(i, 1);
        }

        return true;
    }

    public bool ApplyWeaponRelease(Planet planet, int index, PlanetResearchRules rules)
    {
        var remaining = planet.Research.GetRemainingWeaponResearch(index);
        if (remaining <= 0)
        {
            return false;
        }

        planet.Research.SetRemainingWeaponResearch(index, remaining - 1, rules);
        planet.Research.SetWeaponReleaseProgressSum(planet.Research.ComputeResearchSum());
        return true;
    }

    public bool ApplyWeaponReleaseForAllCategories(Planet planet, PlanetResearchRules rules)
    {
        var changed = false;
        for (var i = 0; i < WeaponResearchCategoryCount; i++)
        {
            var remaining = planet.Research.GetRemainingWeaponResearch(i);
            if (remaining <= 0)
            {
                continue;
            }

            planet.Research.SetRemainingWeaponResearch(i, remaining - 1, rules);
            changed = true;
        }

        if (changed)
        {
            planet.Research.SetWeaponReleaseProgressSum(planet.Research.ComputeResearchSum());
        }

        return changed;
    }

    public void ResetUpdateFlags(Planet planet)
    {
        planet.Research.ResetUpdateFlags();
    }
}
