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

        // Salva quais partidas estavam em andamento antes
        var inProgressBefore = await context.Matches
            .Where(m => m.Status == MatchStatus.InProgress)
            .Select(m => m.Id)
            .ToListAsync();

        await footballApi.SyncMatchResultsAsync();

        // Processa pontuação dos jogos que acabaram
        var finishedMatches = await context.Matches
            .Where(m => m.Status == MatchStatus.Finished && inProgressBefore.Contains(m.Id))
            .ToListAsync();

        foreach (var match in finishedMatches)
        {
            await predictionService.ProcessMatchPredictionsAsync(match.Id);
            logger.LogInformation("Jogo {MatchId} processado: {Home}-{Away}", match.Id, match.HomeScore, match.AwayScore);
        }

        if (finishedMatches.Count > 0)
        {
            await pushService.SendToAllAsync(
                "Resultado disponível!",
                $"{finishedMatches.Count} jogo(s) encerrado(s). Veja sua pontuação.",
                new { type = "match_result" });
        }
    }
}
