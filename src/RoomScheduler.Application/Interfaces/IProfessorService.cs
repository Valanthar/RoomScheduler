using RoomScheduler.Application.DTOs.Professors;

namespace RoomScheduler.Application.Interfaces;

public interface IProfessorService
{
    Task<ProfessorResponse> CreateAsync(CreateProfessorRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<ProfessorResponse>> GetAllAsync(CancellationToken ct = default);
}
