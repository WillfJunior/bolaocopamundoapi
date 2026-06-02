using System.Security.Claims;
using BolaoCopaMundo.Application.DTOs.Notification;
using BolaoCopaMundo.Domain.Entities;
using BolaoCopaMundo.Infrastructure.Data;
using BolaoCopaMundo.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BolaoCopaMundo.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController(
    AppDbContext context,
    PushNotificationService pushService) : ControllerBase
{
    [HttpGet("vapid-public-key")]
    [AllowAnonymous]
    public IActionResult GetPublicKey()
    {
        var key = pushService.GetPublicKey();
        if (key is null) return NotFound(new { error = "VAPID não configurado." });
        return Ok(new { publicKey = key });
    }

    [HttpPost("subscribe")]
    public async Task<IActionResult> Subscribe([FromBody] PushSubscriptionRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var existing = await context.PushSubscriptions
            .FirstOrDefaultAsync(s => s.UserId == userId && s.Endpoint == request.Endpoint);

        if (existing is not null) return Ok();

        context.PushSubscriptions.Add(new PushSubscription
        {
            UserId = userId,
            Endpoint = request.Endpoint,
            P256dh = request.P256dh,
            Auth = request.Auth,
            DeviceInfo = request.DeviceInfo,
            CreatedAt = DateTime.UtcNow
        });

        await context.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete("unsubscribe")]
    public async Task<IActionResult> Unsubscribe([FromQuery] string endpoint)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var sub = await context.PushSubscriptions
            .FirstOrDefaultAsync(s => s.UserId == userId && s.Endpoint == endpoint);

        if (sub is not null)
        {
            context.PushSubscriptions.Remove(sub);
            await context.SaveChangesAsync();
        }

        return NoContent();
    }
}
