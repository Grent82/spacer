namespace Spacer.Infrastructure.Services;

using Spacer.Application.Ports;
using Spacer.Application.Services;
using Spacer.Domain.ValueObjects;

public sealed class StubFleetPostureProvider : IFleetPostureProvider
{
    public FleetPostureSummary GetForRuler(EntityId rulerId) => FleetPostureSummary.Empty;
}
