using Tutorial8.Models.DTOs;

namespace Tutorial8.Services;

public interface ITripsService
{
    Task<List<TripDTO>> GetTripsAsync(CancellationToken cancellationToken);
    Task<TripDTO> GetTripAsync(int id,CancellationToken cancellationToken);
    Task<bool> DoesTripExistAsync(int id,CancellationToken cancellationToken);
    Task<bool> IsTripFullAsync(int id,CancellationToken cancellationToken);
}