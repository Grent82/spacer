namespace Spacer.Application.Ports;

using Spacer.Application.DTOs;

public interface IPlanetFleetSpecStore
{
    void Save(PlanetFleetSpecSet specSet);
}
