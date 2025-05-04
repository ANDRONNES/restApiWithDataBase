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

        string command = "SELECT * FROM Trip";
        /*string command = @"SELECT t.IdTrip,t.Name,c.Name from Trip t 
                           join Country_Trip ct on t.IdTrip = ct.IdTrip
                           join Country c on ct.IdCountry = c.IdCountry";*/
        
        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(command, conn))
        {
            await conn.OpenAsync();

            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    // int idOrdinal = reader.GetOrdinal("IdTrip");
                    trips.Add(new TripDTO()
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        Description = reader.GetString(2),
                        DateFrom = reader.GetDateTime(3),
                        DateTo = reader.GetDateTime(4),
                        MaxPeople = reader.GetInt32(5),
                        Countries = new List<CountryDTO>()
                    });
                }
            }
        }
        

        return trips;
    }
}