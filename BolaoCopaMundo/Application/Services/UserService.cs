using BolaoCopaMundo.Application.DTOs.User;
using BolaoCopaMundo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BolaoCopaMundo.Application.Services;

public class UserService(AppDbContext context, IWebHostEnvironment env, ILogger<UserService> logger)
{
    private static readonly string[] AllowedImageExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

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
            throw new InvalidOperationException("Arquivo excede o limite de 5 MB.");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedImageExtensions.Contains(ext))
            throw new InvalidOperationException("Formato de imagem não suportado. Use JPG, PNG ou WebP.");

        var user = await context.Users.FindAsync(userId)
            ?? throw new KeyNotFoundException("Usuário não encontrado.");

        var photosPath = Path.Combine(env.WebRootPath, "photos");
        Directory.CreateDirectory(photosPath);

        // Remove foto anterior se existir
        if (!string.IsNullOrWhiteSpace(user.PhotoUrl))
        {
            var oldFile = Path.Combine(env.WebRootPath, user.PhotoUrl.TrimStart('/'));
            if (File.Exists(oldFile)) File.Delete(oldFile);
        }

        var fileName = $"{userId}{ext}";
        var filePath = Path.Combine(photosPath, fileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        user.PhotoUrl = $"/photos/{fileName}";
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
