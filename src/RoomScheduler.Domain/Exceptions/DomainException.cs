namespace RoomScheduler.Domain.Exceptions;

/// <summary>
/// Base class for all domain-rule violations.
/// Caught by the global exception handler and mapped to HTTP 400/422.
/// Never exposes a stack trace to the client (see ARCHITECTURE.md).
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}
