using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tutorial8.Services;

namespace Tutorial8.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TripsController : ControllerBase
    {
        private readonly ITripsService _tripsService;

        public TripsController(ITripsService tripsService)
        {
            _tripsService = tripsService;
        }
        
        
        /// <summary>
        ///   Zwraca wszystkie wycieczki z BD.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetTrips(CancellationToken cancellationToken)
        {
            var trips = await _tripsService.GetTripsAsync(cancellationToken);
            return Ok(trips);
        }
        
        
        /// <summary>
        ///   Zwraca pojedynczą wycieczkę z BD razem z listą krajów.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTrip(int id, CancellationToken cancellationToken)
        {
            if (await _tripsService.DoesTripExistAsync(id, cancellationToken))
            {
                return NotFound();
            }

            var trip = await _tripsService.GetTripAsync(id, cancellationToken);
            return Ok(trip);
        }
    }
}