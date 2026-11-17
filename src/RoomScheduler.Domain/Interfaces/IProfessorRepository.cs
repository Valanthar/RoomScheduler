using RoomScheduler.Domain.Entities;

namespace RoomScheduler.Domain.Interfaces;

public interface IProfessorRepository
{
    Task<Professor?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Professor?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<IReadOnlyList<Professor>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(Professor professor, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
