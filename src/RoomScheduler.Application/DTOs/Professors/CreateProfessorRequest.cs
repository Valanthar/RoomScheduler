using RoomScheduler.Domain.Enums;

namespace RoomScheduler.Application.DTOs.Professors;

public record CreateProfessorRequest(string Name, string Email, string Password, UserRole Role = UserRole.Professor);
