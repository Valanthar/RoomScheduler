namespace RoomScheduler.Domain.Exceptions;

/// <summary>
/// Thrown when an authenticated user attempts an action they do not own
/// or lack the role for (Business Rules 4 and 7).
/// Mapped to HTTP 403 Forbidden by the global exception handler.
/// </summary>
public sealed class ForbiddenException : DomainException
{
    public ForbiddenException(string message = "Access denied.") : base(message) { }
}
