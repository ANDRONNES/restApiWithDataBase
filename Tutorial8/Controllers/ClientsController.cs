using Microsoft.AspNetCore.Mvc;
using Tutorial8.Exceptions;
using Tutorial8.Models.DTOs;
using Tutorial8.Services;

namespace Tutorial8.Controllers;

[ApiController]
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
    
    
    
    /// <summary>
    ///   Zwraca wszystkich klientów z BD.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetClientsTrips(CancellationToken cancellationToken)
    {
        var clients = await _clientsService.GetClientsAsync(cancellationToken);
        return Ok(clients);
    }
    
    
    /// <summary>
    ///   Zwraca pojedynczego klienta z BD.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetClient(int id,CancellationToken cancellationToken)
    {
        if (!await _clientsService.DoesClientExistAsync(id,cancellationToken))
        {
            throw new NotFoundException("Client not found");
        }

        var client = await _clientsService.GetClientAsync(id,cancellationToken);
        return Ok(client);
    }

    /// <summary>
    ///   Zwraca imie,nazwisko klienta z listą jego wycieczek.
    /// </summary>
    [HttpGet("{id}/trips")]
    public async Task<IActionResult> GetClientTrips(int id,CancellationToken cancellationToken)
    {
        if (!await _clientsService.DoesClientExistAsync(id,cancellationToken))
        {
            throw new NotFoundException("Client not found");
        }

        var clientsTrips = await _clientsService.GetClientsTripsAsync(id,cancellationToken);
        return Ok(clientsTrips);
    }
    
    /// <summary>
    ///   Tworzy nowego klienta i dodaje go do BD.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateClientDTO dto,CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            throw new BadRequestException("Bad Request");
        }

        int newId = await _clientsService.CreateUserAsync(dto,cancellationToken);
        return Created("", new { Id = newId });
    }

    
    /// <summary>
    ///   Rejestruje klienta na wycieczkę.
    /// </summary>
    [HttpPut("{id}/trips/{tripId}")]
    public async Task<IActionResult> RegisterClientOnTrip(int id, int tripId,CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            throw new BadRequestException("Bad Request");
        }

        if (!await _clientsService.DoesClientExistAsync(id,cancellationToken))
        {
            throw new NotFoundException("Client not found");
        }

        if (!await _tripsService.DoesTripExistAsync(tripId,cancellationToken))
        {
            throw new NotFoundException("Trip not found");
        }

        if (await _tripsService.IsTripFullAsync(tripId,cancellationToken))
        {
            throw new ConflictException("Trip is full");
        }

        if (await _clientsService.IsClientAlreadyOnThisTripAsync(id, tripId,cancellationToken))
        {
            throw new ConflictException("Client is already on this trip");
        }

        await _clientsService.RegisterClientOnTripAsync(id, tripId,cancellationToken);
        return Created("", new { message = "Client has been registered on this trip" });
    }
    
    
    /// <summary>
    ///   Usuwa klienta z wycieczki.
    /// </summary>
    [HttpDelete("{id}/trips/{tripId}")]
    public async Task<IActionResult> DeleteRegestrationFromTrip(int id, int tripId,CancellationToken cancellationToken)
    {
        if (!await _clientsService.DoesClientExistAsync(id,cancellationToken))
        {
            throw new NotFoundException("Client not found");
        }

        if (!await _tripsService.DoesTripExistAsync(tripId,cancellationToken))
        {
            throw new NotFoundException("Trip not found");
        }

        if (!await _clientsService.IsClientAlreadyOnThisTripAsync(id, tripId,cancellationToken))
        {
            throw new NotFoundException("Client is not registered on this trip");
        }
        
        var result = await _clientsService.DeleteClientFromTripAsync(id, tripId, cancellationToken);
        return result ? Ok(new { message = "Client has been deleted from this trip" }) : throw new InternalServerErrorException("Exception occured");
    }
}