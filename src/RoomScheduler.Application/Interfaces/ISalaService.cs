using RoomScheduler.Application.DTOs.Salas;

namespace RoomScheduler.Application.Interfaces;

public interface ISalaService
{
    Task<IReadOnlyList<SalaGridResponse>> GetGridAsync(DateOnly date, CancellationToken ct = default);
}
