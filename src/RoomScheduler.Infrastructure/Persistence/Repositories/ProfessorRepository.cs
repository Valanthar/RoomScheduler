using Microsoft.EntityFrameworkCore;
using RoomScheduler.Domain.Entities;
using RoomScheduler.Domain.Interfaces;

namespace RoomScheduler.Infrastructure.Persistence.Repositories;

public sealed class ProfessorRepository : IProfessorRepository
{
    private readonly AppDbContext _db;

    public ProfessorRepository(AppDbContext db) => _db = db;

    public Task<Professor?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.Professors.FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<Professor?> GetByEmailAsync(string email, CancellationToken ct = default)
        => _db.Professors.FirstOrDefaultAsync(p => p.Email == email.ToLowerInvariant(), ct);

    public async Task<IReadOnlyList<Professor>> GetAllAsync(CancellationToken ct = default)
        => await _db.Professors.AsNoTracking().ToListAsync(ct);

    public async Task AddAsync(Professor professor, CancellationToken ct = default)
        => await _db.Professors.AddAsync(professor, ct);

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}
