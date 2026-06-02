namespace BolaoCopaMundo.Application.DTOs.Auth;

public record AuthResponse(string Token, DateTime ExpiresAt, UserInfo User);

public record UserInfo(Guid Id, string Name, string PhoneNumber, string? PhotoUrl, bool IsAdmin);
