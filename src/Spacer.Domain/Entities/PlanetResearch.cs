namespace Spacer.Domain.Entities;

using System;
using Spacer.Domain.ValueObjects;

public sealed class PlanetResearch
{
    private readonly int[] _currentLevels = new int[10];
    private readonly int[] _updatedResearch = new int[10];
    private readonly int[] _updatedWeaponResearch = new int[3];
    private readonly int[] _remainingWeaponResearches = new int[3];

    public int SystemTechLevel { get; private set; }
    public int WeaponReleaseProgressSum { get; private set; }

    public int GetCurrentLevel(int index) => _currentLevels[index];
    public int GetUpdatedResearch(int index) => _updatedResearch[index];
    public int GetUpdatedWeaponResearch(int index) => _updatedWeaponResearch[index];
    public int GetRemainingWeaponResearch(int index) => _remainingWeaponResearches[index];
    public bool IsWeaponResearchUpdated(int index) => _updatedWeaponResearch[index] > 0;

    public int ComputeResearchSum()
    {
        var sum = 0;
        for (var i = 0; i < _currentLevels.Length; i++)
        {
            sum += _currentLevels[i] / 1000;
        }
        return sum;
    }

    public void SetCurrentLevel(int index, int value, PlanetResearchRules rules)
    {
        _currentLevels[index] = Clamp(value, rules.MinResearchLevel, rules.MaxResearchLevel);
    }

    public void SetUpdatedWeaponResearch(int index, int value)
    {
        _updatedWeaponResearch[index] = Math.Max(0, value);
    }

    public void SetUpdatedResearch(int index, int value)
    {
        _updatedResearch[index] = Math.Max(0, value);
    }

    public void SetRemainingWeaponResearch(int index, int value, PlanetResearchRules rules)
    {
        _remainingWeaponResearches[index] = Clamp(value, rules.MinWeaponReleaseRemaining, rules.MaxWeaponReleaseRemaining);
    }

    public void SetSystemTechLevel(int value, PlanetResearchRules rules)
    {
        SystemTechLevel = Clamp(value, rules.MinSystemTechLevel, rules.MaxSystemTechLevel);
    }

    public void SetWeaponReleaseProgressSum(int value)
    {
        WeaponReleaseProgressSum = Math.Max(0, value);
    }

    public void CopyFrom(PlanetResearch other)
    {
        Array.Copy(other._currentLevels, _currentLevels, _currentLevels.Length);
        Array.Copy(other._updatedResearch, _updatedResearch, _updatedResearch.Length);
        Array.Copy(other._updatedWeaponResearch, _updatedWeaponResearch, _updatedWeaponResearch.Length);
        Array.Copy(other._remainingWeaponResearches, _remainingWeaponResearches, _remainingWeaponResearches.Length);
        SystemTechLevel = other.SystemTechLevel;
        WeaponReleaseProgressSum = other.WeaponReleaseProgressSum;
    }

    public void ResetUpdateFlags()
    {
        Array.Fill(_updatedResearch, 0);
        Array.Fill(_updatedWeaponResearch, 0);
    }

    private static int Clamp(int value, int min, int max)
    {
        if (value < min)
        {
            return min;
        }
        if (value > max)
        {
            return max;
        }
        return value;
    }
}
