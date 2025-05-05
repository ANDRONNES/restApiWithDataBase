using Microsoft.Data.SqlClient;
using Tutorial8.Models.DTOs;

namespace Tutorial8.Services;

public class ClientsService : IClientsService
{
    private readonly string _connectionString;

    public ClientsService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    // public Task<List<ClientDTO>> GetClientsTrips()
    // {
    //     throw new NotImplementedException();
    // }
    public async Task<List<ClientDTO>> GetClients()
    {
        List<ClientDTO> clients = new List<ClientDTO>();
        string command = "SELECT * FROM Client";
        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(command, conn))
        {
            await conn.OpenAsync();
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    clients.Add(new ClientDTO
                    {
                        Id = reader.GetInt32(0),
                        FirstName = reader.GetString(1),
                        LastName = reader.GetString(2),
                        Email = reader.GetString(3),
                        Telephone = reader.GetString(4),
                        Pesel = reader.GetString(5),
                    });
                }
            }
        }

        return clients;
    }

    public async Task<ClientDTO> GetClient(string id)
    {
        ClientDTO client = null;
        string command = "SELECT * FROM Client Where IdClient = @Id";
        using(SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(command, conn))
        {
            cmd.Parameters.AddWithValue("@Id", id);
            await conn.OpenAsync();
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    client = new ClientDTO
                    {
                        Id = reader.GetInt32(0),
                        FirstName = reader.GetString(1),
                        LastName = reader.GetString(2),
                        Email = reader.GetString(3),
                        Telephone = reader.GetString(4),
                        Pesel = reader.GetString(5),
                    };
                }
            }
        }
        return client;
    }

    public async Task<bool> DoesClientExist(string id)
    {
        string command = "SELECT COUNT(*) FROM Client Where IdClient = @id";
        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(command, conn))
        {
            cmd.Parameters.AddWithValue("@id", id);

            await conn.OpenAsync();
            int count = (int)await cmd.ExecuteScalarAsync();
            return count > 0;
        }
    }
}