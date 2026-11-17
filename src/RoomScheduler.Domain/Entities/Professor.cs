using RoomScheduler.Domain.Enums;
using RoomScheduler.Domain.Exceptions;

namespace RoomScheduler.Domain.Entities;

/// <summary>
/// Aggregate root representing an authenticated system user.
/// 
/// Invariants enforced by this entity:
///   - Name and email cannot be null, empty, or whitespace.
///   - Email must contain '@'.
///   - Password is never stored in plaintext; only the hash is held.
///   - Role is explicit and defaults to Professor (least privilege).
/// 
/// Business Rules covered: 1, 2, 3, 4.
/// </summary>
public sealed class Professor : Entity<Guid>
{
    public string Name         { get; private set; } = default!;
    public string Email        { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public UserRole Role       { get; private set; }
    public DateTime CreatedAt  { get; private init; }

    // ── EF Core parameterless constructor (private — never called by application code) ──
    private Professor() { }

    /// <summary>
    /// Creates a new Professor. The <paramref name="passwordHash"/> must already
    /// be produced by IPasswordHasher — raw passwords are rejected by convention.
    /// </summary>
    public static Professor Create(
        string   name,
        string   email,
        string   passwordHash,
        UserRole role = UserRole.Professor)
    {
        ValidateName(name);
        ValidateEmail(email);
        ValidateHash(passwordHash);

        return new Professor
        {
            Id           = Guid.NewGuid(),
            Name         = name.Trim(),
            Email        = email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash,
            Role         = role,
            CreatedAt    = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Replaces the stored password hash after re-hashing in the application/infra layer.
    /// </summary>
    public void UpdatePasswordHash(string newHash)
    {
        ValidateHash(newHash);
        PasswordHash = newHash;
    }

    // ── Private guards ──────────────────────────────────────────────────────────

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Professor name cannot be empty.");
        if (name.Trim().Length < 2)
            throw new DomainException("Professor name must have at least 2 characters.");
    }

    private static void ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("Professor email cannot be empty.");
        if (!email.Contains('@'))
            throw new DomainException("Professor email is not valid.");
    }

    private static void ValidateHash(string hash)
    {
        if (string.IsNullOrWhiteSpace(hash))
            throw new DomainException("Password hash cannot be empty.");
    }
}
