namespace RoomScheduler.Application.DTOs.Reservas;

public record UpdateReservaRequest(string Subject, DateTime StartsAt, DateTime EndsAt);
