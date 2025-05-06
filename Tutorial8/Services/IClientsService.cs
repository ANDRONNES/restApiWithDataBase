using Tutorial8.Models.DTOs;

namespace Tutorial8.Services;

public interface IClientsService
{
    Task<List<ClientDTO>> GetClients();
    Task<ClientDTO> GetClient(string id);
    Task<bool> DoesClientExist(string id);
    Task<ClientTripDTO> GetClientsTrips(string id);
    Task<int> CreateUser(CreateClientDTO dto);
    Task RegisterClientOnTrip(string id, int tripId);
}