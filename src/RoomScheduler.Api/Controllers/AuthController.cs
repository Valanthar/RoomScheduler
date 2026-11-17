using Microsoft.AspNetCore.Mvc;
using RoomScheduler.Application.DTOs.Auth;
using RoomScheduler.Application.Interfaces;

namespace RoomScheduler.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth) => _auth = auth;

    /// <summary>POST /api/v1/auth/login — Public (Business Rule 3)</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request, CancellationToken ct)
    {
        var response = await _auth.LoginAsync(request, ct);
        return Ok(response);
    }
}
