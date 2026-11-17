using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoomScheduler.Application.DTOs.Professors;
using RoomScheduler.Application.Interfaces;

namespace RoomScheduler.Api.Controllers;

[ApiController]
[Route("api/v1/professores")]
[Authorize]
public sealed class ProfessoresController : ControllerBase
{
    private readonly IProfessorService _service;

    public ProfessoresController(IProfessorService service) => _service = service;

    /// <summary>POST /api/v1/professores — Admin only (Business Rules 1 & 4)</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ProfessorResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create(
        [FromBody] CreateProfessorRequest request, CancellationToken ct)
    {
        var response = await _service.CreateAsync(request, ct);
        return CreatedAtAction(null, new { id = response.Id }, response);
    }
}
