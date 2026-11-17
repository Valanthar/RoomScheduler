using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoomScheduler.Application.DTOs.Salas;
using RoomScheduler.Application.Interfaces;

namespace RoomScheduler.Api.Controllers;

[ApiController]
[Route("api/v1/salas")]
[Authorize]
public sealed class SalasController : ControllerBase
{
    private readonly ISalaService _service;

    public SalasController(ISalaService service) => _service = service;

    /// <summary>GET /api/v1/salas/grid?data=YYYY-MM-DD — Any authenticated user</summary>
    [HttpGet("grid")]
    [ProducesResponseType(typeof(IReadOnlyList<SalaGridResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetGrid(
        [FromQuery] DateOnly data, CancellationToken ct)
    {
        var grid = await _service.GetGridAsync(data, ct);
        return Ok(grid);
    }
}
