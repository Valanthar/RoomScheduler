namespace RoomScheduler.Application.DTOs.Reservas;

public record ReservaResponse(
    Guid     Id,
    Guid     SalaId,
    string   SalaName,
    Guid     ProfessorId,
    string   ProfessorName,
    string   Subject,
    DateTime StartsAt,
    DateTime EndsAt,
    DateTime CreatedAt);
