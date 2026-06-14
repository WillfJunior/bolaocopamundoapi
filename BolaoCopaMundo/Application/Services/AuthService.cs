using BolaoCopaMundo.Application.DTOs.Auth;
using BolaoCopaMundo.Domain.Entities;
using BolaoCopaMundo.Infrastructure.Data;
using BolaoCopaMundo.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace BolaoCopaMundo.Application.Services;

public class AuthService(AppDbContext context, TokenService tokenService, PushNotificationService pushService)
{
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var exists = await context.Users.AnyAsync(u => u.PhoneNumber == request.PhoneNumber);
        if (exists)
            throw new InvalidOperationException("Telefone já cadastrado.");

        var user = new User
        {
            Name = request.Name,
            PhoneNumber = request.PhoneNumber,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        return BuildAuthResponse(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.PhoneNumber == request.PhoneNumber && u.IsActive);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Telefone ou senha incorretos.");

        return BuildAuthResponse(user);
    }

    public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request)
    {
        var user = await context.Users.FindAsync(userId)
            ?? throw new KeyNotFoundException("Usuário não encontrado.");

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            throw new UnauthorizedAccessException("Senha atual incorreta.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();
    }

    public async Task<ForgotPasswordResponse> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.PhoneNumber == request.PhoneNumber && u.IsActive)
            ?? throw new KeyNotFoundException("Usuário com este telefone não encontrado.");

        var tempPassword = GenerateTemporaryPassword();

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(tempPassword);
        user.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();

        await pushService.SendToUserAsync(
            user.Id,
            "Bolão Copa 2026 ⚽ - Senha Recuperada",
            $"Sua nova senha temporária é: {tempPassword}\n\nFaça login e altere para uma senha de sua preferência.");

        return new ForgotPasswordResponse(
            "Senha temporária enviada para você via notificação push.",
            user.Name);
    }

    private AuthResponse BuildAuthResponse(User user)
    {
        var (token, expiresAt) = tokenService.GenerateToken(user);
        return new AuthResponse(
            token,
            expiresAt,
            new UserInfo(user.Id, user.Name, user.PhoneNumber, user.PhotoUrl, user.IsAdmin));
    }

    private static string GenerateTemporaryPassword(int length = 12)
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789!@#$%";
        var random = new Random();
        return new string(Enumerable.Range(0, length)
            .Select(_ => chars[random.Next(chars.Length)])
            .ToArray());
    }
}
