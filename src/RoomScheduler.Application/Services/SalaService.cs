using RoomScheduler.Application.DTOs.Reservas;
using RoomScheduler.Application.DTOs.Salas;
using RoomScheduler.Application.Interfaces;
using RoomScheduler.Domain.Interfaces;

namespace RoomScheduler.Application.Services;

public sealed class SalaService : ISalaService
{
    private readonly ISalaRepository _salaRepo;

    public SalaService(ISalaRepository salaRepo) => _salaRepo = salaRepo;

    public async Task<IReadOnlyList<SalaGridResponse>> GetGridAsync(DateOnly date, CancellationToken ct = default)
    {
        var salas = await _salaRepo.GetGridForDateAsync(date, ct);

        return salas.Select(s => new SalaGridResponse(
            s.Id,
            s.Name,
            s.Capacity,
            s.Reservas.Select(r => new ReservaResponse(
                r.Id, r.SalaId, s.Name,
                r.ProfessorId,
                r.Professor?.Name ?? "Unknown",
                r.Subject, r.StartsAt, r.EndsAt, r.CreatedAt))
            .OrderBy(r => r.StartsAt)
            .ToList()
            .AsReadOnly()
        )).ToList().AsReadOnly();
    }
}
