namespace RoomScheduler.Domain.Exceptions;

/// <summary>
/// Thrown when an aggregate root cannot be found by its identity.
/// Mapped to HTTP 404 Not Found by the global exception handler.
/// </summary>
public sealed class NotFoundException : DomainException
{
    public NotFoundException(string entityName, object id)
        : base($"{entityName} with id '{id}' was not found.") { }
}
