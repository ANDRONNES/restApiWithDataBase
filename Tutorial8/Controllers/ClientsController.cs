using Microsoft.AspNetCore.Mvc;
using Tutorial8.Models.DTOs;
using Tutorial8.Services;

namespace Tutorial8.Controllers;

[ApiController]
// [Route("api/[controller]/{id}/trips")]
[Route("api/[controller]")]
public class ClientsController : ControllerBase
{
    private readonly ITripsService _tripsService;
    private readonly IClientsService _clientsService;

    public ClientsController(IClientsService clientsService, ITripsService tripsService)
    {
        _clientsService = clientsService;
        _tripsService = tripsService;
    }

    [HttpGet]
    public async Task<IActionResult> GetClientsTrips()
    {
        var clients = await _clientsService.GetClients();
        return Ok(clients);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetClient(int id)
    {
        if (!await _clientsService.DoesClientExist(id))
        {
            return NotFound();
        }

        var client = await _clientsService.GetClient(id);
        return Ok(client);
    }


    [HttpGet("{id}/trips")]
    public async Task<IActionResult> GetClientTrips(int id)
    {
        if (!await _clientsService.DoesClientExist(id))
        {
            return NotFound();
        }

        var clientsTrips = await _clientsService.GetClientsTrips(id);
        return Ok(clientsTrips);
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateClientDTO dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            int newId = await _clientsService.CreateUser(dto);
            return Created("", new { Id = newId });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception e)
        {
            return BadRequest(new { message = e.Message });
        }
    }


    [HttpPut("{id}/trips/{tripId}")]
    public async Task<IActionResult> RegisterClientOnTrip(int id, int tripId)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        if (!await _clientsService.DoesClientExist(id))
        {
            return Conflict("Client not found");
        }

        if (!await _tripsService.DoesTripExist(tripId))
        {
            return Conflict("Trip not found");
        }

        if (await _tripsService.IsTripFull(tripId))
        {
            return Conflict("Trip is full");
        }

        if (await _clientsService.IsClientAlreadyOnThisTrip(id,tripId))
        {
            return Conflict("Client is already on this trip");
        }

        await _clientsService.RegisterClientOnTrip(id, tripId);
        return Created();
    }

    [HttpDelete("{id}/trips/{tripId}")]
    public async Task<IActionResult> DeleteRegestrationFromTrip(int id, int tripId)
    {
        _clientsService.DeleteClientFromTrip(id, tripId);
        return Ok();
    }
}