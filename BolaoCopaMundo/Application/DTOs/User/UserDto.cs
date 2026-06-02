namespace BolaoCopaMundo.Application.DTOs.User;

public record UserDto(
    Guid Id,
    string Name,
    string PhoneNumber,
    string? PhotoUrl,
    bool IsAdmin,
    DateTime CreatedAt
);
