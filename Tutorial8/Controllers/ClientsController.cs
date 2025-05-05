using Microsoft.AspNetCore.Mvc;
using Tutorial8.Services;

namespace Tutorial8.Controllers;

[ApiController]
// [Route("api/[controller]/{id}/trips")]
[Route("api/[controller]")]
public class ClientsController : ControllerBase
{
    private readonly IClientsService _clientsService;

    public ClientsController(IClientsService clientsService)
    {
        _clientsService = clientsService;
    }

    [HttpGet]
    public async Task<IActionResult> GetClientsTrips()
    {
        var clients = await _clientsService.GetClients();
        return Ok(clients);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetClient(string id)
    {
        if (!await _clientsService.DoesClientExist(id))
        {
            return NotFound();
        }
        var client = await _clientsService.GetClient(id);
        return Ok(client);
    }
}