using BolaoCopaMundo.Application.DTOs.Prediction;
using BolaoCopaMundo.Domain.Entities;
using BolaoCopaMundo.Domain.Enums;
using BolaoCopaMundo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BolaoCopaMundo.Application.Services;

public class PredictionService(AppDbContext context)
{
    public async Task<List<PredictionDto>> GetUserPredictionsAsync(Guid userId)
    {
        return await context.Predictions
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.UpdatedAt)
            .Select(p => ToDto(p))
            .ToListAsync();
    }

    public async Task<PredictionDto?> GetUserPredictionForMatchAsync(Guid userId, int matchId)
    {
        var prediction = await context.Predictions
            .FirstOrDefaultAsync(p => p.UserId == userId && p.MatchId == matchId);

        return prediction is null ? null : ToDto(prediction);
    }

    public async Task<PredictionDto> SavePredictionAsync(Guid userId, SavePredictionRequest request)
    {
        var match = await context.Matches.FindAsync(request.MatchId)
            ?? throw new KeyNotFoundException("Jogo não encontrado.");

        if (match.Status != MatchStatus.Scheduled)
            throw new InvalidOperationException("Não é possível palpitar em jogos que já começaram ou encerraram.");

        if (match.MatchDate <= DateTime.UtcNow.AddHours(1))
            throw new InvalidOperationException("O prazo para palpitar neste jogo encerrou. Palpites são bloqueados 1 hora antes do início.");

        if (match.HomeTeamId is null || match.AwayTeamId is null)
            throw new InvalidOperationException("Os times deste jogo ainda não foram definidos.");

        var existing = await context.Predictions
            .FirstOrDefaultAsync(p => p.UserId == userId && p.MatchId == request.MatchId);

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

        foreach (var p in predictions)
        {
            p.Points = ScoringService.CalculatePoints(
                p.HomeScore, p.AwayScore,
                match.HomeScore.Value, match.AwayScore.Value);
            p.IsProcessed = true;
            p.UpdatedAt = DateTime.UtcNow;
        }

        await context.SaveChangesAsync();
    }

    private static PredictionDto ToDto(Prediction p) =>
        new(p.Id, p.MatchId, p.HomeScore, p.AwayScore, p.Points, p.IsProcessed, p.CreatedAt, p.UpdatedAt);
}
