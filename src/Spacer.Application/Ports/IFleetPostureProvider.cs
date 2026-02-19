namespace Spacer.Application.Ports;

using Spacer.Application.Services;
using Spacer.Domain.ValueObjects;

public interface IFleetPostureProvider
{
    FleetPostureSummary GetForRuler(EntityId rulerId);
}
