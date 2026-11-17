using RoomScheduler.Application.DTOs.Reservas;

namespace RoomScheduler.Application.Interfaces;

public interface IReservaService
{
    Task<ReservaResponse> CreateAsync(Guid professorId, CreateReservaRequest request, CancellationToken ct = default);
    Task<ReservaResponse> UpdateAsync(Guid professorId, Guid reservaId, UpdateReservaRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid professorId, Guid reservaId, CancellationToken ct = default);
}
