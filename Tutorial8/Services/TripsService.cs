using Microsoft.Data.SqlClient;
using Tutorial8.Models.DTOs;

namespace Tutorial8.Services;

public class TripsService : ITripsService
{
    private readonly string _connectionString;

    public TripsService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public async Task<List<TripDTO>> GetTrips()
    {
        var trips = new List<TripDTO>();

        // string command = "SELECT * FROM Trip";
        string command = @"SELECT t.IdTrip, t.Name, t.Description, t.DateFrom, t.DateTo, t.MaxPeople, c.Name 
                           FROM Trip t
                           JOIN Country_Trip ct ON t.IdTrip = ct.IdTrip
                           JOIN Country c ON ct.IdCountry = c.IdCountry
                           ORDER BY t.IdTrip";

        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(command, conn))
        {
            await conn.OpenAsync();

            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    int idOrdinal = reader.GetInt32(0);
                    var trip = trips.FirstOrDefault(t => t.Id == idOrdinal);

                    if (trip == null)
                    {
                        trip = new TripDTO
                        {
                            Id = idOrdinal,
                            Name = reader.GetString(1),
                            Description = reader.GetString(2),
                            DateFrom = reader.GetDateTime(3),
                            DateTo = reader.GetDateTime(4),
                            MaxPeople = reader.GetInt32(5),
                            Countries = new List<CountryDTO>()
                        };

                        trips.Add(trip);
                    }

                    trip.Countries.Add(new CountryDTO
                    {
                        Name = reader.GetString(6)
                    });
                }
            }
        }

        return trips;
    }

    public async Task<TripDTO> GetTrip(int id)
    {
        string queryForTrip = "Select * from trip where IdTrip = @id";
        string queryForCountry = @"SELECT c.Name
                                   FROM Country c
                                   JOIN Country_Trip ct ON c.IdCountry = ct.IdCountry
                                   WHERE ct.IdTrip = @id";
        TripDTO trip = null;

        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            await conn.OpenAsync();
            using (SqlCommand cmd = new SqlCommand(queryForTrip, conn))
            {
                cmd.Parameters.AddWithValue("@id", id);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        int idOrdinal = reader.GetInt32(0);
                        trip = new TripDTO
                        {
                            Id = idOrdinal,
                            Name = reader.GetString(1),
                            Description = reader.GetString(2),
                            DateFrom = reader.GetDateTime(3),
                            DateTo = reader.GetDateTime(4),
                            MaxPeople = reader.GetInt32(5),
                            Countries = new List<CountryDTO>()
                        };
                    }
                }
            }

            using (SqlCommand cmd2 = new SqlCommand(queryForCountry, conn))
            {
                cmd2.Parameters.AddWithValue("@id", id);
                using (var reader = await cmd2.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        string name = reader.GetString(0);
                        trip.Countries.Add(new CountryDTO
                        {
                            Name = name
                        });
                    }
                }
            }
        }

        return trip;
    }

    public async Task<bool> DoesTripExist(int id)
    {
        string command = "SELECT COUNT(*) FROM Trip Where IdTrip = @id";
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
