namespace Spacer.Application.Services;

using System.Collections.Generic;
using Spacer.Application.Ports;

public sealed class WeaponNameFormatter : IWeaponNameFormatter
{
    private readonly IFactionCatalog _factionCatalog;

    public WeaponNameFormatter(IFactionCatalog factionCatalog)
    {
        _factionCatalog = factionCatalog;
    }

    public string BuildName(int factionId, int shipTypeCode, int tier)
    {
        var code = ResolveFactionCode(factionId);
        var classShort = ResolveClassShort(shipTypeCode);
        return $"{code}-{classShort}-T{tier}";
    }

    public string BuildImageKey(int factionId, int shipTypeCode, int tier)
    {
        var code = ResolveFactionCode(factionId).ToLowerInvariant();
        var classShort = ResolveClassShort(shipTypeCode).ToLowerInvariant();
        return $"{code}_{classShort}_{tier}";
    }

    private string ResolveFactionCode(int factionId)
    {
        var info = _factionCatalog.FindById(factionId);
        if (info is null || string.IsNullOrWhiteSpace(info.Value.Code))
        {
            return $"F{factionId}";
        }

        return info.Value.Code.Trim();
    }

    private static string ResolveClassShort(int shipTypeCode)
    {
        return ShipClassShortNames.TryGetValue(shipTypeCode, out var shortName)
            ? shortName
            : $"C{shipTypeCode}";
    }

    private static readonly Dictionary<int, string> ShipClassShortNames = new()
    {
        [100] = "BS",
        [101] = "CR",
        [102] = "MS",
        [103] = "LP",
        [104] = "CV",
        [105] = "DS",
        [200] = "SF",
        [300] = "GT",
        [400] = "FT"
    };
}
