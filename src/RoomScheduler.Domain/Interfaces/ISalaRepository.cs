using RoomScheduler.Domain.Entities;

namespace RoomScheduler.Domain.Interfaces;

public interface ISalaRepository
{
    Task<Sala?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Sala>> GetAllActiveAsync(CancellationToken ct = default);

    /// <summary>
    /// Returns all rooms with their reservations pre-loaded for the given date.
    /// Used to build the daily grid (GET /salas/grid) and to feed the domain's
    /// overlap check without a second round-trip.
    /// </summary>
    Task<IReadOnlyList<Sala>> GetGridForDateAsync(DateOnly date, CancellationToken ct = default);

    Task AddAsync(Sala sala, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
