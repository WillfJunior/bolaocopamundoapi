using System.Text.Json;
using BolaoCopaMundo.Domain.Enums;
using BolaoCopaMundo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BolaoCopaMundo.Infrastructure.Services;

public class FootballApiService(
    HttpClient httpClient,
    AppDbContext context,
    ILogger<FootballApiService> logger,
    IConfiguration configuration)
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public bool IsConfigured => !string.IsNullOrWhiteSpace(configuration["FootballApi:ApiKey"]);

    public async Task<int> SyncMatchResultsAsync()
    {
        if (!IsConfigured)
        {
            logger.LogWarning("FootballApi:ApiKey não configurada. Sincronização ignorada.");
            return 0;
        }

        try
        {
            var season = configuration["FootballApi:Season"] ?? "2026";
            var response = await httpClient.GetAsync(
                $"competitions/WC/matches?season={season}&status=FINISHED,IN_PLAY");

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Football API retornou {StatusCode}", response.StatusCode);
                return 0;
            }

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<FootballApiResponse>(content, JsonOptions);

            if (result?.Matches is null) return 0;

            int updated = 0;
            foreach (var apiMatch in result.Matches)
            {
                var match = await context.Matches
                    .Include(m => m.HomeTeam)
                    .Include(m => m.AwayTeam)
                    .FirstOrDefaultAsync(m =>
                        m.HomeTeam!.FifaCode == apiMatch.HomeTeam.Tla &&
                        m.AwayTeam!.FifaCode == apiMatch.AwayTeam.Tla &&
                        m.MatchDate.Date == DateTime.Parse(apiMatch.UtcDate).Date);

                if (match is null) continue;

                var newStatus = apiMatch.Status switch
                {
                    "FINISHED"                              => MatchStatus.Finished,
                    "IN_PLAY" or "PAUSED" or "HALFTIME"    => MatchStatus.InProgress,
                    _                                       => match.Status
                };

                if (newStatus == match.Status &&
                    match.HomeScore == apiMatch.Score?.FullTime?.Home &&
                    match.AwayScore == apiMatch.Score?.FullTime?.Away)
                    continue;

                match.Status    = newStatus;
                match.ExternalId ??= apiMatch.Id.ToString();

                if (apiMatch.Score?.FullTime?.Home is not null)
                {
                    match.HomeScore = apiMatch.Score.FullTime.Home;
                    match.AwayScore = apiMatch.Score.FullTime.Away;
                }

                updated++;
            }

            await context.SaveChangesAsync();
            logger.LogInformation("Football API: {Count} jogos atualizados.", updated);
            return updated;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao sincronizar resultados da Football API.");
            return 0;
        }
    }

    // Response models — football-data.org v4
    private record FootballApiResponse(List<FootballApiMatch> Matches);
    private record FootballApiMatch(int Id, string UtcDate, string Status, FootballApiTeam HomeTeam, FootballApiTeam AwayTeam, FootballApiScore? Score);
    private record FootballApiTeam(int Id, string Name, string Tla);
    private record FootballApiScore(FootballApiScoreDetail? FullTime);
    private record FootballApiScoreDetail(int? Home, int? Away);
}
