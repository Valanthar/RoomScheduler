using Microsoft.EntityFrameworkCore;
using RoomScheduler.Domain.Entities;
using RoomScheduler.Domain.Interfaces;

namespace RoomScheduler.Infrastructure.Persistence.Repositories;

public sealed class SalaRepository : ISalaRepository
{
    private readonly AppDbContext _db;

    public SalaRepository(AppDbContext db) => _db = db;

    public Task<Sala?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.Salas.FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task<IReadOnlyList<Sala>> GetAllActiveAsync(CancellationToken ct = default)
        => await _db.Salas.Where(s => s.IsActive).AsNoTracking().ToListAsync(ct);

    public async Task<IReadOnlyList<Sala>> GetGridForDateAsync(DateOnly date, CancellationToken ct = default)
    {
        var start = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var end   = date.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);

        return await _db.Salas
            .Where(s => s.IsActive)
            .Include(s => s.Reservas
                .Where(r => r.StartsAt >= start && r.StartsAt <= end))
                .ThenInclude(r => r.Professor)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task AddAsync(Sala sala, CancellationToken ct = default)
        => await _db.Salas.AddAsync(sala, ct);

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}
