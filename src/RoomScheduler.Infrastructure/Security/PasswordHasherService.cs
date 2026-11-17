using Microsoft.AspNetCore.Identity;
using RoomScheduler.Application.Interfaces;
using RoomScheduler.Domain.Entities;

namespace RoomScheduler.Infrastructure.Security;

/// <summary>
/// Wraps ASP.NET Core's PasswordHasher which uses PBKDF2 with a random salt
/// and 100,000 iterations (Business Rule 2).
/// 
/// We pass a dummy Professor instance because PasswordHasher<T> requires a user
/// object for its API — in PBKDF2 mode it is not actually used in the computation.
/// </summary>
public sealed class PasswordHasherService : IPasswordHasherService
{
    private static readonly PasswordHasher<Professor> _hasher = new();
    private static readonly Professor _dummy = Professor.Create("Dummy", "dummy@d.com", "placeholder");

    public string Hash(string plainTextPassword)
        => _hasher.HashPassword(_dummy, plainTextPassword);

    public bool Verify(string plainTextPassword, string hash)
    {
        var result = _hasher.VerifyHashedPassword(_dummy, hash, plainTextPassword);
        return result != PasswordVerificationResult.Failed;
    }
}
