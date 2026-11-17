using RoomScheduler.Domain.Entities;

namespace RoomScheduler.Application.Interfaces;

/// <summary>
/// Generates a signed JWT for an authenticated professor.
/// Defined in Application so services can depend on it without referencing Infrastructure.
/// </summary>
public interface IJwtService
{
    (string Token, DateTime ExpiresAt) GenerateToken(Professor professor);
}
