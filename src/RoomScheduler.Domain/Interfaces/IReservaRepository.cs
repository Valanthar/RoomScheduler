using RoomScheduler.Domain.Entities;

namespace RoomScheduler.Domain.Interfaces;

public interface IReservaRepository
{
    Task<Reserva?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Returns all reservations for a room on a given date.
    /// The Application layer passes this list into Reserva.Create / Reserva.Update
    /// so the domain can perform the pure overlap check.
    /// </summary>
    Task<IReadOnlyList<Reserva>> GetByRoomAndDateAsync(
        Guid salaId, DateOnly date, CancellationToken ct = default);

    Task AddAsync(Reserva reserva, CancellationToken ct = default);
    void Remove(Reserva reserva);
    Task SaveChangesAsync(CancellationToken ct = default);
}
