using Microsoft.EntityFrameworkCore;
using RoomScheduler.Domain.Entities;
using RoomScheduler.Domain.Interfaces;

namespace RoomScheduler.Infrastructure.Persistence.Repositories;

public sealed class ReservaRepository : IReservaRepository
{
    private readonly AppDbContext _db;

    public ReservaRepository(AppDbContext db) => _db = db;

    public Task<Reserva?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.Reservas.FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task<IReadOnlyList<Reserva>> GetByRoomAndDateAsync(
        Guid salaId, DateOnly date, CancellationToken ct = default)
    {
        var start = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var end   = date.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);

        return await _db.Reservas
            .Where(r => r.SalaId   == salaId &&
                        r.StartsAt >= start  &&
                        r.StartsAt <= end)
            .ToListAsync(ct);
    }

    public async Task AddAsync(Reserva reserva, CancellationToken ct = default)
        => await _db.Reservas.AddAsync(reserva, ct);

    public void Remove(Reserva reserva)
        => _db.Reservas.Remove(reserva);

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}
