using BolaoCopaMundo.Domain.Enums;
using BolaoCopaMundo.Infrastructure.Data;
using BolaoCopaMundo.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace BolaoCopaMundo.Infrastructure.Jobs;

public class SendMatchRemindersJob(
    AppDbContext context,
    PushNotificationService pushService,
    ILogger<SendMatchRemindersJob> logger)
{
    public async Task ExecuteAsync()
    {
        var soon = DateTime.UtcNow.AddHours(1);
        var window = DateTime.UtcNow.AddHours(1).AddMinutes(5);

        var upcomingMatches = await context.Matches
            .Include(m => m.HomeTeam)
            .Include(m => m.AwayTeam)
            .Where(m => m.Status == MatchStatus.Scheduled &&
                        m.MatchDate >= soon && m.MatchDate <= window)
            .ToListAsync();

        foreach (var match in upcomingMatches)
        {
            var home = match.HomeTeam?.Name ?? "?";
            var away = match.AwayTeam?.Name ?? "?";

            logger.LogInformation("Enviando lembrete para o jogo {Home} x {Away}", home, away);

            await pushService.SendToAllAsync(
                "Jogo em 1 hora!",
                $"{home} x {away} — não esqueça de fazer seu palpite!",
                new { type = "match_reminder", matchId = match.Id });
        }
    }
}
