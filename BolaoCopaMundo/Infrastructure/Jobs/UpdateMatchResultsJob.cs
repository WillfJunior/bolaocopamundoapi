using BolaoCopaMundo.Application.Services;
using BolaoCopaMundo.Domain.Enums;
using BolaoCopaMundo.Infrastructure.Data;
using BolaoCopaMundo.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace BolaoCopaMundo.Infrastructure.Jobs;

public class UpdateMatchResultsJob(
    FootballApiService footballApi,
    PredictionService predictionService,
    PushNotificationService pushService,
    AppDbContext context,
    ILogger<UpdateMatchResultsJob> logger)
{
    public async Task ExecuteAsync()
    {
        logger.LogInformation("Job: sincronizando resultados...");

        // Busca jogos em andamento ou que terminaram recentemente (últimas 3h)
        var hasActiveMatches = await context.Matches.AnyAsync(m =>
            m.Status == MatchStatus.InProgress ||
            (m.Status == MatchStatus.Scheduled && m.MatchDate <= DateTime.UtcNow.AddHours(3)));

        if (!hasActiveMatches)
        {
            logger.LogDebug("Nenhum jogo ativo no momento. Pulando sincronização.");
            return;
        }

        await footballApi.SyncMatchResultsAsync();

        // Processa todos os jogos Finished que ainda têm predições não processadas
        // e cuja notificação push ainda não foi enviada
        var matchesWithUnprocessed = await context.Matches
            .Where(m => m.Status == MatchStatus.Finished
                     && !m.PushNotificationSent
                     && m.Predictions.Any(p => !p.IsProcessed))
            .Select(m => m.Id)
            .ToListAsync();

        foreach (var matchId in matchesWithUnprocessed)
        {
            await predictionService.ProcessMatchPredictionsAsync(matchId);
            var match = await context.Matches.FindAsync(matchId);
            logger.LogInformation("Jogo {MatchId} processado: {Home}-{Away}", matchId, match?.HomeScore, match?.AwayScore);
        }

        if (matchesWithUnprocessed.Count > 0)
        {
            await pushService.SendToAllAsync(
                "Resultado disponível!",
                $"{matchesWithUnprocessed.Count} jogo(s) encerrado(s). Veja sua pontuação.",
                new { type = "match_result" });

            // Marca que o push foi enviado para esses jogos
            var matchesToMark = await context.Matches
                .Where(m => matchesWithUnprocessed.Contains(m.Id))
                .ToListAsync();

            foreach (var m in matchesToMark)
                m.PushNotificationSent = true;

            await context.SaveChangesAsync();
        }
    }
}
