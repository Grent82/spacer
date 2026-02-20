namespace Spacer.Domain.Entities;

using System;
using Spacer.Domain.Enums;
using Spacer.Domain.ValueObjects;

public sealed class ItemSpec
{
    private ItemSpec(EntityId id, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name must be provided.", nameof(name));
        }

        Id = id;
        Name = name;
    }

    public static ItemSpec Create(EntityId id, string name) => new(id, name);
    public static ItemSpec Create(int id, string name) => new(EntityId.Create(id), name);

    public EntityId Id { get; }
    public string Name { get; private set; }

    public string NativeName { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public ItemEffectType EffectType { get; private set; }
    public int EffectValue { get; private set; }
    public ItemUsageType UsageType { get; private set; }
    public int DistributionCount { get; private set; }

    public void ConfigureDefinition( string nativeName, string description, ItemEffectType effectType, int effectValue, ItemUsageType usageType, int distributionCount )
    {
        if (!Enum.IsDefined(typeof(ItemEffectType), effectType))
        {
            throw new ArgumentOutOfRangeException(nameof(effectType));
        }
        if (!Enum.IsDefined(typeof(ItemUsageType), usageType))
        {
            throw new ArgumentOutOfRangeException(nameof(usageType));
        }
        if (distributionCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(distributionCount));
        }

        NativeName = nativeName ?? string.Empty;
        Description = description ?? string.Empty;
        EffectType = effectType;
        EffectValue = effectValue;
        UsageType = usageType;
        DistributionCount = distributionCount;
    }
}
