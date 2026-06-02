using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BolaoCopaMundo.Domain.Entities;
using Microsoft.IdentityModel.Tokens;

namespace BolaoCopaMundo.Infrastructure.Services;

public class TokenService(IConfiguration configuration)
{
    public (string token, DateTime expiresAt) GenerateToken(User user)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(configuration["Jwt:Secret"]!));

        var expiresAt = DateTime.UtcNow.AddDays(
            int.Parse(configuration["Jwt:ExpirationDays"] ?? "7"));

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("name", user.Name),
            new Claim("phone", user.PhoneNumber),
            new Claim("admin", user.IsAdmin.ToString().ToLower()),
        };

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiresAt,
            Issuer = configuration["Jwt:Issuer"],
            Audience = configuration["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        };

        var handler = new JwtSecurityTokenHandler();
        var token = handler.CreateToken(descriptor);
        return (handler.WriteToken(token), expiresAt);
    }
}
