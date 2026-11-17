using RoomScheduler.Domain.Enums;

namespace RoomScheduler.Application.DTOs.Professors;

public record ProfessorResponse(Guid Id, string Name, string Email, UserRole Role, DateTime CreatedAt);
