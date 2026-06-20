using BolaoCopaMundo.Application.DTOs.Prediction;
using BolaoCopaMundo.Domain.Entities;
using BolaoCopaMundo.Domain.Enums;
using BolaoCopaMundo.Infrastructure.Data;
using BolaoCopaMundo.Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace BolaoCopaMundo.Application.Services;

public class PredictionService(AppDbContext context, IMemoryCache cache, IHubContext<RankingHub> hubContext)
{
    private static readonly TimeZoneInfo Brt =
        TimeZoneInfo.FindSystemTimeZoneById(
            OperatingSystem.IsWindows() ? "E. South America Standard Time" : "America/Sao_Paulo");

    public async Task<List<PredictionDto>> GetUserPredictionsAsync(Guid userId, Guid groupId)
    {
        return await context.Predictions
            .Where(p => p.UserId == userId && p.GroupId == groupId)
            .OrderByDescending(p => p.UpdatedAt)
            .Select(p => ToDto(p))
            .ToListAsync();
    }

    public async Task<PredictionDto?> GetUserPredictionForMatchAsync(Guid userId, int matchId, Guid groupId)
    {
        var prediction = await context.Predictions
            .FirstOrDefaultAsync(p => p.UserId == userId && p.MatchId == matchId && p.GroupId == groupId);

        return prediction is null ? null : ToDto(prediction);
    }

    public async Task<PredictionDto> SavePredictionAsync(Guid userId, SavePredictionRequest request)
    {
        var isMember = await context.BolaoGroupMembers.AnyAsync(m =>
            m.GroupId == request.GroupId && m.UserId == userId && m.Status == MemberStatus.Active);

        if (!isMember)
            throw new InvalidOperationException("Você não é membro ativo deste grupo.");

        var match = await context.Matches.FindAsync(request.MatchId)
            ?? throw new KeyNotFoundException("Jogo não encontrado.");

        if (match.Status != MatchStatus.Scheduled)
            throw new InvalidOperationException("Não é possível palpitar em jogos que já começaram ou encerraram.");

        var matchDateBrt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(match.MatchDate, DateTimeKind.Utc), Brt);
        var nowBrt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc), Brt);

        if (matchDateBrt <= nowBrt.AddHours(1))
            throw new InvalidOperationException("O prazo para palpitar neste jogo encerrou. Palpites são bloqueados 1 hora antes do início.");

        if (match.HomeTeamId is null || match.AwayTeamId is null)
            throw new InvalidOperationException("Os times deste jogo ainda não foram definidos.");

        var existing = await context.Predictions
            .FirstOrDefaultAsync(p => p.UserId == userId && p.MatchId == request.MatchId && p.GroupId == request.GroupId);

        if (existing is not null)
        {
            existing.HomeScore = request.HomeScore;
            existing.AwayScore = request.AwayScore;
            existing.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync();
            return ToDto(existing);
        }

        var prediction = new Prediction
        {
            UserId = userId,
            GroupId = request.GroupId,
            MatchId = request.MatchId,
            HomeScore = request.HomeScore,
            AwayScore = request.AwayScore,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Predictions.Add(prediction);
        await context.SaveChangesAsync();
        return ToDto(prediction);
    }

    public async Task ProcessMatchPredictionsAsync(int matchId)
    {
        var match = await context.Matches.FindAsync(matchId);
        if (match?.HomeScore is null || match.AwayScore is null)
            return;

        var predictions = await context.Predictions
            .Where(p => p.MatchId == matchId && !p.IsProcessed)
            .ToListAsync();

        if (predictions.Count == 0)
            return;

        var affectedGroupIds = new HashSet<Guid>();

        foreach (var p in predictions)
        {
            p.Points = ScoringService.CalculatePoints(
                p.HomeScore, p.AwayScore,
                match.HomeScore.Value, match.AwayScore.Value);
            p.IsProcessed = true;
            p.UpdatedAt = DateTime.UtcNow;
            affectedGroupIds.Add(p.GroupId);
        }

        await context.SaveChangesAsync();

        cache.Remove("ranking:global");
        foreach (var groupId in affectedGroupIds)
            cache.Remove($"ranking:group:{groupId}");

        await hubContext.Clients.All.SendAsync("rankings-updated", matchId);

        foreach (var groupId in affectedGroupIds)
        {
            await hubContext.Clients.Group($"group-ranking-{groupId}")
                .SendAsync("group-ranking-updated", groupId, matchId);
        }

        await hubContext.Clients.Group("global-ranking")
            .SendAsync("global-ranking-updated", matchId);
    }

    private static PredictionDto ToDto(Prediction p) =>
        new(p.Id, p.GroupId, p.MatchId, p.HomeScore, p.AwayScore, p.Points, p.IsProcessed, p.CreatedAt, p.UpdatedAt);
}
