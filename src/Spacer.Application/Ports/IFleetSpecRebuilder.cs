namespace Spacer.Application.Ports;

using Spacer.Application.DTOs;
using Spacer.Domain.Entities;

public interface IFleetSpecRebuilder
{
    void RebuildFleetSpecs(Planet planet, WeaponResearchCategory category);
}
