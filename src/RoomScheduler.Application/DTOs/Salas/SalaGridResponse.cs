using RoomScheduler.Application.DTOs.Reservas;

namespace RoomScheduler.Application.DTOs.Salas;

public record SalaGridResponse(Guid Id, string Name, int Capacity, IReadOnlyList<ReservaResponse> Reservas);
