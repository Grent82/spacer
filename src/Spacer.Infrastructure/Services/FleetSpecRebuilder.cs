namespace Spacer.Infrastructure.Services;

using Spacer.Application.DTOs;
using Spacer.Application.Ports;
using Spacer.Application.Services;
using Spacer.Domain.Entities;

public sealed class FleetSpecRebuilder : IFleetSpecRebuilder
{
    private readonly FleetSpecRebuildService _service;

    public FleetSpecRebuilder(FleetSpecRebuildService service)
    {
        _service = service;
    }

    public void RebuildFleetSpecs(Planet planet, WeaponResearchCategory category)
    {
        _service.Rebuild(planet, category);
    }
}
