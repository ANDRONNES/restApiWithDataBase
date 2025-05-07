using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Microsoft.Data.SqlClient;
using Tutorial8.Models.DTOs;

namespace Tutorial8.Services;

public class ClientsService : IClientsService
{
    private readonly string _connectionString;
    private readonly ITripsService _tripsService;

    public ClientsService(IConfiguration configuration, ITripsService tripsService)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
        _tripsService = tripsService;
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

    public async Task<ClientDTO> GetClient(int id)
    {
        ClientDTO client = null;
        string command = "SELECT * FROM Client Where IdClient = @Id";
        using (SqlConnection conn = new SqlConnection(_connectionString))
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

    public async Task<bool> DoesClientExist(int id)
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

    public async Task<ClientTripDTO> GetClientsTrips(int id)
    {
        ClientTripDTO clientsTrips = null;
        string queryForClient = "SELECT IdClient,FirstName,LastName FROM Client Where IdClient = @Id";
        string command =
            @"SELECT t.IdTrip,t.Name,t.Description,t.DateFrom,t.DateTo,t.MaxPeople,clt.RegisteredAt,clt.PaymentDate
              FROM Client cl JOIN Client_Trip clt ON cl.IdClient = clt.IdClient
              JOIN Trip t ON clt.IdTrip = t.IdTrip
              WHERE cl.IdClient = @Id
              Order by cl.IdClient";
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            await conn.OpenAsync();
            using (SqlCommand cmd0 = new SqlCommand(queryForClient, conn))
            {
                cmd0.Parameters.AddWithValue("@Id", id);
                using (var reader = await cmd0.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        clientsTrips = new ClientTripDTO
                        {
                            Id = reader.GetInt32(0),
                            FirstName = reader.GetString(1),
                            LastName = reader.GetString(2),
                            Trips = new List<TripClientDTO>()
                        };
                    }
                }
            }

            using (SqlCommand cmd = new SqlCommand(command, conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        clientsTrips.Trips.Add(new TripClientDTO
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Description = reader.GetString(2),
                            DateFrom = reader.GetDateTime(3),
                            DateTo = reader.GetDateTime(4),
                            MaxPeople = reader.GetInt32(5),
                            RegisteredAt = reader.GetInt32(6),
                            PaymentDate = reader.GetInt32(7)
                        });
                    }
                }
            }
        }

        return clientsTrips;
    }

    public async Task<int> CreateUser(CreateClientDTO dto)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            await conn.OpenAsync();
            string checkPeselUnique = "Select Count(*) from Client Where Pesel = @Pesel";
            using (var checkCmd = new SqlCommand(checkPeselUnique, conn))
            {
                checkCmd.Parameters.AddWithValue("@Pesel", dto.Pesel);
                var count = (int)await checkCmd.ExecuteScalarAsync();
                if (count > 0)
                {
                    throw new InvalidOperationException("Client with this PESEL already exists.");
                }
            }

            string checkEmailUnique = "Select Count(*) from Client Where Email = @Email";
            using (var checkCmd = new SqlCommand(checkEmailUnique, conn))
            {
                checkCmd.Parameters.AddWithValue("@Email", dto.Email);
                var count = (int)await checkCmd.ExecuteScalarAsync();
                if (count > 0)
                {
                    throw new InvalidOperationException("Client with this Email already exists.");
                }
            }

            string checkPhoneUnique = "Select Count(*) from Client Where Telephone = @Telephone";
            using (var checkCmd = new SqlCommand(checkPhoneUnique, conn))
            {
                checkCmd.Parameters.AddWithValue("@Telephone", dto.Telephone);
                var count = (int)await checkCmd.ExecuteScalarAsync();
                if (count > 0)
                {
                    throw new InvalidOperationException("Client with this Phone already exists.");
                }
            }

            string command = @"INSERT INTO Client (FirstName, LastName, Email, Telephone, Pesel) 
                           VALUES(@FirstName, @LastName, @Email, @Telephone, @Pesel)
                           SELECT SCOPE_IDENTITY()";
            using (SqlCommand cmd = new SqlCommand(command, conn))
            {
                cmd.Parameters.AddWithValue("@FirstName", dto.FirstName);
                cmd.Parameters.AddWithValue("@LastName", dto.LastName);
                cmd.Parameters.AddWithValue("@Email", dto.Email);
                cmd.Parameters.AddWithValue("@Telephone", dto.Telephone);
                cmd.Parameters.AddWithValue("@Pesel", dto.Pesel);

                var id = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(id);
            }
        }
    }

    public async Task RegisterClientOnTrip(int id, int tripId)
    {
        string query = @"INSERT INTO Client_Trip(IdClient, IdTrip, RegisteredAt) 
                       VALUES(@IdClient, @IdTrip, @RegisteredAt)";

        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            await conn.OpenAsync();
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@IdClient", id);
                cmd.Parameters.AddWithValue("@IdTrip", tripId);
                int currDate = int.Parse(DateTime.Now.ToString("yyyyMMdd"));
                cmd.Parameters.AddWithValue("@RegisteredAt", currDate);
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }

    public async Task<bool> IsClientAlreadyOnThisTrip(int id, int tripId)
    {
        int count = 0;
        string query = "SELECT * FROM Client_Trip Where IdClient = @IdClient AND IdTrip = @IdTrip";
        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(query, conn))
        {
            await conn.OpenAsync();
            cmd.Parameters.AddWithValue("@IdClient", id);
            cmd.Parameters.AddWithValue("@IdTrip", tripId);

            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    count = reader.GetInt32(0);
                }
            }
        }
        return count > 0;
    }

    public async Task<bool> DeleteClientFromTrip(int id, int tripId)
    {
        string query = "Delete From Client_Trip Where IdClient = @IdClient AND IdTrip = @IdTrip";
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@IdClient", id);
                cmd.Parameters.AddWithValue("@IdTrip", tripId);
                await conn.OpenAsync();
                int rowsAffected = await cmd.ExecuteNonQueryAsync();
                return rowsAffected > 0;
            }
        }
    }
}