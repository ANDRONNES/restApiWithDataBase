using Tutorial8.Models.DTOs;

namespace Tutorial8.Services;

public interface IClientsService
{
    Task<List<ClientDTO>> GetClients();
    Task<ClientDTO> GetClient(int id);
    Task<bool> DoesClientExist(int id);
    Task<ClientTripDTO> GetClientsTrips(int id);
    Task<int> CreateUser(CreateClientDTO dto);
    Task RegisterClientOnTrip(int id, int tripId);
    Task<bool> IsClientAlreadyOnThisTrip(int id,int tripId);
    Task<bool> DeleteClientFromTrip(int id,int tripId);
}