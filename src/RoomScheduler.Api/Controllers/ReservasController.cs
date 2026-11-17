using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoomScheduler.Application.DTOs.Reservas;
using RoomScheduler.Application.Interfaces;

namespace RoomScheduler.Api.Controllers;

[ApiController]
[Route("api/v1/reservas")]
[Authorize]
public sealed class ReservasController : ControllerBase
{
    private readonly IReservaService _service;

    public ReservasController(IReservaService service) => _service = service;

    /// <summary>POST /api/v1/reservas — Professor role only (Business Rule 5, 6)</summary>
    [HttpPost]
    [Authorize(Roles = "Professor")]
    [ProducesResponseType(typeof(ReservaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateReservaRequest request, CancellationToken ct)
    {
        var professorId = GetCurrentProfessorId();
        var response    = await _service.CreateAsync(professorId, request, ct);
        return CreatedAtAction(nameof(Create), new { id = response.Id }, response);
    }

    /// <summary>PUT /api/v1/reservas/{id} — Author only (Business Rule 7)</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ReservaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id, [FromBody] UpdateReservaRequest request, CancellationToken ct)
    {
        var professorId = GetCurrentProfessorId();
        var response    = await _service.UpdateAsync(professorId, id, request, ct);
        return Ok(response);
    }

    /// <summary>DELETE /api/v1/reservas/{id} — Author only (Business Rule 7)</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var professorId = GetCurrentProfessorId();
        await _service.DeleteAsync(professorId, id, ct);
        return NoContent();
    }

    // ── Helper: extract the professor's Id from the JWT (Business Rule 7) ───────
    private Guid GetCurrentProfessorId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
               ?? User.FindFirstValue("sub");

        if (!Guid.TryParse(sub, out var id))
            throw new UnauthorizedAccessException("Invalid token subject.");

        return id;
    }
}
