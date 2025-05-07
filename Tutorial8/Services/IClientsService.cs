using Tutorial8.Models.DTOs;

namespace Tutorial8.Services;

public interface IClientsService
{
    Task<List<ClientDTO>> GetClientsAsync(CancellationToken cancellationToken);
    Task<ClientDTO> GetClientAsync(int id,CancellationToken cancellationToken);
    Task<bool> DoesClientExistAsync(int id,CancellationToken cancellationToken);
    Task<ClientTripDTO> GetClientsTripsAsync(int i,CancellationToken cancellationTokend);
    Task<int> CreateUserAsync(CreateClientDTO dto,CancellationToken cancellationToken);
    Task RegisterClientOnTripAsync(int id, int tripId,CancellationToken cancellationToken);
    Task<bool> IsClientAlreadyOnThisTripAsync(int id,int tripId,CancellationToken cancellationToken);
    Task<bool> DeleteClientFromTripAsync(int id,int tripId,CancellationToken cancellationToken);
}