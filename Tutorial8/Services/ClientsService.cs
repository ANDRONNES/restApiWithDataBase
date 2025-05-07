using Microsoft.Data.SqlClient;
using Tutorial8.Exceptions;
using Tutorial8.Models.DTOs;

namespace Tutorial8.Services;

public class ClientsService : IClientsService
{
    private readonly string _connectionString;

    public ClientsService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }
    
    public async Task<List<ClientDTO>> GetClientsAsync(CancellationToken cancellationToken)
    {
        List<ClientDTO> clients = new List<ClientDTO>();
        string command = "SELECT * FROM Client"; //Wsystkie informacje o kliencie
        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(command, conn))
        {
            await conn.OpenAsync(cancellationToken);
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

    public async Task<ClientDTO> GetClientAsync(int id,CancellationToken cancellationToken)
    {
        ClientDTO client = null;
        string command = "SELECT * FROM Client Where IdClient = @Id"; //Wszystkie informacje o kliencie pod tym Id
        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(command, conn))
        {
            cmd.Parameters.AddWithValue("@Id", id);
            await conn.OpenAsync(cancellationToken);
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

    public async Task<bool> DoesClientExistAsync(int id,CancellationToken cancellationToken)
    {
        string command = "SELECT COUNT(*) FROM Client Where IdClient = @id";
        //Sprawdzamy czy istnieje klient o podanym Id
        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(command, conn))
        {
            cmd.Parameters.AddWithValue("@id", id);

            await conn.OpenAsync(cancellationToken);
            int count = (int)await cmd.ExecuteScalarAsync();
            return count > 0;
        }
    }

    public async Task<ClientTripDTO> GetClientsTripsAsync(int id,CancellationToken cancellationToken)
    {
        ClientTripDTO clientsTrips = null;
        string queryForClient = "SELECT IdClient,FirstName,LastName FROM Client Where IdClient = @Id";
        //Otrymujemy id,imie,nazwisko klienta o podanym Id
        string command =
            @"SELECT t.IdTrip,t.Name,t.Description,t.DateFrom,t.DateTo,t.MaxPeople,clt.RegisteredAt,clt.PaymentDate
              FROM Client cl JOIN Client_Trip clt ON cl.IdClient = clt.IdClient
              JOIN Trip t ON clt.IdTrip = t.IdTrip
              WHERE cl.IdClient = @Id
              Order by cl.IdClient";
        //Pobieramy informacje o wycieczce i kilka informacji z Client_Trip
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            await conn.OpenAsync(cancellationToken);
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

    public async Task<int> CreateUserAsync(CreateClientDTO dto, CancellationToken cancellationToken)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            await conn.OpenAsync(cancellationToken);
            string checkPeselUnique = "Select Count(*) from Client Where Pesel = @Pesel";
            //Sprawdzamy czy klient o danym numerze pesel istnieje
            using (var checkCmd = new SqlCommand(checkPeselUnique, conn))
            {
                checkCmd.Parameters.AddWithValue("@Pesel", dto.Pesel);
                var count = (int)await checkCmd.ExecuteScalarAsync();
                if (count > 0)
                {
                    throw new ConflictException("Client with this PESEL already exists.");
                }
            }

            string checkEmailUnique = "Select Count(*) from Client Where Email = @Email";
            //Sprawdzamy czy klient o danym Email istnieje
            using (var checkCmd = new SqlCommand(checkEmailUnique, conn))
            {
                checkCmd.Parameters.AddWithValue("@Email", dto.Email);
                var count = (int)await checkCmd.ExecuteScalarAsync();
                if (count > 0)
                {
                    throw new ConflictException("Client with this Email already exists.");
                }
            }

            string checkPhoneUnique = "Select Count(*) from Client Where Telephone = @Telephone";
            //Sprawdzamy czy klient o danym numerze telefonu instnieje
            using (var checkCmd = new SqlCommand(checkPhoneUnique, conn))
            {
                checkCmd.Parameters.AddWithValue("@Telephone", dto.Telephone);
                var count = (int)await checkCmd.ExecuteScalarAsync();
                if (count > 0)
                {
                    throw new ConflictException("Client with this Phone already exists.");
                }
            }

            string command = @"INSERT INTO Client (FirstName, LastName, Email, Telephone, Pesel) 
                           VALUES(@FirstName, @LastName, @Email, @Telephone, @Pesel)
                           SELECT SCOPE_IDENTITY()";
            //Wstawiamy nowego klienta do bazy danych i zwracamy jego Id
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

    public async Task RegisterClientOnTripAsync(int id, int tripId, CancellationToken cancellationToken)
    {
        string query = @"INSERT INTO Client_Trip(IdClient, IdTrip, RegisteredAt) 
                       VALUES(@IdClient, @IdTrip, @RegisteredAt)";
        //Wstawiamy(Rejestrujemy) Klienta na Wycieczkę, Dodaemy bieżącą datę

        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            await conn.OpenAsync(cancellationToken);
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

    public async Task<bool> IsClientAlreadyOnThisTripAsync(int id, int tripId, CancellationToken cancellationToken)
    {
        string query = "SELECT count(*) FROM Client_Trip Where IdClient = @IdClient AND IdTrip = @IdTrip";
        //Sprawdzamy czy klien jest zarejestrowany na tą wycieczkę
        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(query, conn))
        {
            await conn.OpenAsync(cancellationToken);
            cmd.Parameters.AddWithValue("@IdClient", id);
            cmd.Parameters.AddWithValue("@IdTrip", tripId);
            var count = (int)await cmd.ExecuteScalarAsync();
            return count > 0;
        }
        
    }

    public async Task<bool> DeleteClientFromTripAsync(int id, int tripId,CancellationToken cancellationToken)
    {
        string query = "Delete From Client_Trip Where IdClient = @IdClient AND IdTrip = @IdTrip";
        //Usuwamy Rejestracje klienta z wycieczki
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@IdClient", id);
                cmd.Parameters.AddWithValue("@IdTrip", tripId);
                await conn.OpenAsync(cancellationToken);
                int rowsAffected = await cmd.ExecuteNonQueryAsync();
                return rowsAffected > 0;
            }
        }
    }
}