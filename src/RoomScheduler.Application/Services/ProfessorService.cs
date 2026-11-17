using RoomScheduler.Application.DTOs.Professors;
using RoomScheduler.Application.Interfaces;
using RoomScheduler.Domain.Entities;
using RoomScheduler.Domain.Exceptions;
using RoomScheduler.Domain.Interfaces;

namespace RoomScheduler.Application.Services;

public sealed class ProfessorService : IProfessorService
{
    private readonly IProfessorRepository   _repo;
    private readonly IPasswordHasherService _hasher;

    public ProfessorService(IProfessorRepository repo, IPasswordHasherService hasher)
    {
        _repo   = repo;
        _hasher = hasher;
    }

    public async Task<ProfessorResponse> CreateAsync(CreateProfessorRequest request, CancellationToken ct = default)
    {
        var existing = await _repo.GetByEmailAsync(request.Email, ct);
        if (existing is not null)
            throw new ConflictException($"A professor with email '{request.Email}' already exists.");

        var hash      = _hasher.Hash(request.Password);
        var professor = Professor.Create(request.Name, request.Email, hash, request.Role);

        await _repo.AddAsync(professor, ct);
        await _repo.SaveChangesAsync(ct);

        return MapToResponse(professor);
    }

    public async Task<IReadOnlyList<ProfessorResponse>> GetAllAsync(CancellationToken ct = default)
    {
        var professors = await _repo.GetAllAsync(ct);
        return professors.Select(MapToResponse).ToList().AsReadOnly();
    }

    private static ProfessorResponse MapToResponse(Professor p)
        => new(p.Id, p.Name, p.Email, p.Role, p.CreatedAt);
}
