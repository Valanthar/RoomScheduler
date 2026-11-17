namespace RoomScheduler.Domain.Exceptions;

/// <summary>
/// Thrown when a new reservation overlaps an existing one (Business Rule 5).
/// Mapped to HTTP 409 Conflict by the global exception handler.
/// </summary>
public sealed class ConflictException : DomainException
{
    public ConflictException(string message) : base(message) { }
}
