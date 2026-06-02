using BolaoCopaMundo.Application.DTOs.User;
using BolaoCopaMundo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BolaoCopaMundo.Application.Services;

public class UserService(AppDbContext context, ILogger<UserService> logger)
{
    private static readonly string[] AllowedImageExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private static readonly Dictionary<string, string> MimeTypes = new()
    {
        [".jpg"]  = "image/jpeg",
        [".jpeg"] = "image/jpeg",
        [".png"]  = "image/png",
        [".webp"] = "image/webp"
    };
    private const long MaxFileSizeBytes = 1 * 1024 * 1024; // 1 MB

    public async Task<UserDto> GetProfileAsync(Guid userId)
    {
        var user = await context.Users.FindAsync(userId)
            ?? throw new KeyNotFoundException("Usuário não encontrado.");

        return ToDto(user);
    }

    public async Task<UserDto> UpdateProfileAsync(Guid userId, UpdateUserRequest request)
    {
        var user = await context.Users.FindAsync(userId)
            ?? throw new KeyNotFoundException("Usuário não encontrado.");

        if (!string.IsNullOrWhiteSpace(request.PhoneNumber) &&
            request.PhoneNumber != user.PhoneNumber)
        {
            var taken = await context.Users.AnyAsync(u =>
                u.PhoneNumber == request.PhoneNumber && u.Id != userId);

            if (taken)
                throw new InvalidOperationException("Telefone já está em uso.");

            user.PhoneNumber = request.PhoneNumber;
        }

        user.Name = request.Name;
        user.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();

        return ToDto(user);
    }

    public async Task<string> UploadPhotoAsync(Guid userId, IFormFile file)
    {
        if (file.Length == 0)
            throw new InvalidOperationException("Arquivo vazio.");

        if (file.Length > MaxFileSizeBytes)
            throw new InvalidOperationException("Arquivo excede o limite de 1 MB. Comprima a imagem antes de enviar.");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedImageExtensions.Contains(ext))
            throw new InvalidOperationException("Formato de imagem não suportado. Use JPG, PNG ou WebP.");

        var user = await context.Users.FindAsync(userId)
            ?? throw new KeyNotFoundException("Usuário não encontrado.");

        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        var base64 = Convert.ToBase64String(ms.ToArray());
        var mime = MimeTypes[ext];

        user.PhotoUrl = $"data:{mime};base64,{base64}";
        user.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();

        logger.LogInformation("Foto atualizada para usuário {UserId}", userId);
        return user.PhotoUrl;
    }

    public async Task<List<UserDto>> GetAllAsync()
    {
        return await context.Users
            .Where(u => u.IsActive)
            .Select(u => ToDto(u))
            .ToListAsync();
    }

    private static UserDto ToDto(Domain.Entities.User u) =>
        new(u.Id, u.Name, u.PhoneNumber, u.PhotoUrl, u.IsAdmin, u.CreatedAt);
}
