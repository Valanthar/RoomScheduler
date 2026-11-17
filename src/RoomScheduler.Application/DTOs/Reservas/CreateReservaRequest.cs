namespace RoomScheduler.Application.DTOs.Reservas;

public record CreateReservaRequest(Guid SalaId, string Subject, DateTime StartsAt, DateTime EndsAt);
