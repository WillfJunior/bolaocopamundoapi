using System.Text.Json;
using BolaoCopaMundo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using WebPush;

namespace BolaoCopaMundo.Infrastructure.Services;

public class PushNotificationService(
    AppDbContext context,
    IConfiguration configuration,
    ILogger<PushNotificationService> logger)
{
    private VapidDetails? GetVapidDetails()
    {
        var subject = configuration["Vapid:Subject"];
        var publicKey = configuration["Vapid:PublicKey"];
        var privateKey = configuration["Vapid:PrivateKey"];

        if (string.IsNullOrWhiteSpace(subject) ||
            string.IsNullOrWhiteSpace(publicKey) ||
            string.IsNullOrWhiteSpace(privateKey))
            return null;

        return new VapidDetails(subject, publicKey, privateKey);
    }

    public string? GetPublicKey() => configuration["Vapid:PublicKey"];

    public async Task SendToUserAsync(Guid userId, string title, string body, object? data = null)
    {
        var vapid = GetVapidDetails();
        if (vapid is null) return;

        var subscriptions = await context.PushSubscriptions
            .Where(s => s.UserId == userId)
            .ToListAsync();

        await SendToSubscriptionsAsync(subscriptions, title, body, data, vapid);
    }

    public async Task<bool> SendWelcomeAsync(Guid userId)
    {
        var vapid = GetVapidDetails();

        var subscriptions = await context.PushSubscriptions
            .Where(s => s.UserId == userId)
            .ToListAsync();

        if (subscriptions.Count == 0) return false;

        if (vapid is null) return true;

        var payload = JsonSerializer.Serialize(new
        {
            title = "Bolão Copa 2026 ⚽",
            body  = "Boa! Agora você fica por dentro de tudo — palpites, resultados e ranking em tempo real. Bora ganhar! 🏆"
        });

        var stale = new List<Guid>();
        foreach (var sub in subscriptions)
        {
            try
            {
                var client = new WebPushClient();
                var webSub = new WebPush.PushSubscription(sub.Endpoint, sub.P256dh, sub.Auth);
                await client.SendNotificationAsync(webSub, payload, vapid);
            }
            catch (WebPushException ex) when (
                ex.StatusCode is System.Net.HttpStatusCode.NotFound or
                                 System.Net.HttpStatusCode.Gone)
            {
                stale.Add(sub.Id);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Falha ao enviar welcome push para {Endpoint}", sub.Endpoint);
            }
        }

        if (stale.Count > 0)
        {
            context.PushSubscriptions.RemoveRange(
                context.PushSubscriptions.Where(s => stale.Contains(s.Id)));
            await context.SaveChangesAsync();
        }

        return true;
    }

    public async Task SendToAllAsync(string title, string body, object? data = null)
    {
        var vapid = GetVapidDetails();
        if (vapid is null) return;

        var subscriptions = await context.PushSubscriptions.ToListAsync();
        await SendToSubscriptionsAsync(subscriptions, title, body, data, vapid);
    }

    private async Task SendToSubscriptionsAsync(
        List<Domain.Entities.PushSubscription> subscriptions,
        string title, string body, object? data, VapidDetails vapid)
    {
        var payload = JsonSerializer.Serialize(new { title, body, data });
        var stale = new List<Guid>();

        foreach (var sub in subscriptions)
        {
            try
            {
                var client = new WebPushClient();
                var webSub = new WebPush.PushSubscription(sub.Endpoint, sub.P256dh, sub.Auth);
                await client.SendNotificationAsync(webSub, payload, vapid);
            }
            catch (WebPushException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Gone)
            {
                stale.Add(sub.Id);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Falha ao enviar push para {Endpoint}", sub.Endpoint);
            }
        }

        if (stale.Count > 0)
        {
            context.PushSubscriptions.RemoveRange(
                context.PushSubscriptions.Where(s => stale.Contains(s.Id)));
            await context.SaveChangesAsync();
        }
    }
}
