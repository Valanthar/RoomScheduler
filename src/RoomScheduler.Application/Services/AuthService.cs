using RoomScheduler.Application.DTOs.Auth;
using RoomScheduler.Application.Interfaces;
using RoomScheduler.Domain.Exceptions;
using RoomScheduler.Domain.Interfaces;

namespace RoomScheduler.Application.Services;

public sealed class AuthService : IAuthService
{
    private readonly IProfessorRepository  _professorRepo;
    private readonly IPasswordHasherService _hasher;
    private readonly IJwtService            _jwt;

    public AuthService(
        IProfessorRepository  professorRepo,
        IPasswordHasherService hasher,
        IJwtService            jwt)
    {
        _professorRepo = professorRepo;
        _hasher        = hasher;
        _jwt           = jwt;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var professor = await _professorRepo.GetByEmailAsync(request.Email, ct);

        // Intentionally vague error — do not reveal whether the email exists (Business Rule 3).
        if (professor is null || !_hasher.Verify(request.Password, professor.PasswordHash))
            throw new DomainException("Invalid credentials.");

        var (token, expiresAt) = _jwt.GenerateToken(professor);
        return new LoginResponse(token, expiresAt);
    }
}
