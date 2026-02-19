namespace Spacer.Domain.Services;

using Spacer.Domain.Entities;
using Spacer.Domain.ValueObjects;

public sealed class PlanetResearchService
{
    private const int WeaponResearchCategoryCount = 3;

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
