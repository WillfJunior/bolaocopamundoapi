using BolaoCopaMundo.Application.DTOs.Ranking;
using BolaoCopaMundo.Domain.Enums;
using BolaoCopaMundo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace BolaoCopaMundo.Application.Services;

public class RankingService(AppDbContext context, IMemoryCache cache)
{
    private const string RANKING_CACHE_KEY = "ranking:global";
    private static readonly TimeSpan CACHE_DURATION = TimeSpan.FromSeconds(60);

    public async Task<List<RankingEntryDto>> GetRankingAsync()
    {
        if (cache.TryGetValue(RANKING_CACHE_KEY, out List<RankingEntryDto>? cachedRanking))
            return cachedRanking!;

        var raw = await context.Users
            .Where(u => u.IsActive)
            .Select(u => new
            {
                u.Id,
                u.Name,
                u.PhotoUrl,
                TotalPoints = u.Predictions.Where(p => p.IsProcessed).Sum(p => p.Points),
                ExactScores = u.Predictions.Count(p => p.IsProcessed && p.Points == 3),
                CorrectOutcomes = u.Predictions.Count(p => p.IsProcessed && p.Points == 1),
                TotalPredictions = u.Predictions.Count(),
                Errors = u.Predictions.Count(p => p.IsProcessed && p.Points == 0)
            })
            .OrderByDescending(u => u.TotalPoints)
            .ThenByDescending(u => u.ExactScores)
            .ThenByDescending(u => u.CorrectOutcomes)
            .ThenBy(u => u.Name)
            .ToListAsync();

        var result = raw.Select((entry, index) => new RankingEntryDto(
            Position: index + 1,
            UserId: entry.Id,
            UserName: entry.Name,
            UserPhotoUrl: entry.PhotoUrl,
            TotalPoints: entry.TotalPoints,
            ExactScores: entry.ExactScores,
            CorrectOutcomes: entry.CorrectOutcomes,
            TotalPredictions: entry.TotalPredictions,
            Errors: entry.TotalPredictions - entry.ExactScores - entry.CorrectOutcomes
        )).ToList();

        cache.Set(RANKING_CACHE_KEY, result, CACHE_DURATION);
        return result;
    }

    public async Task<RankingEntryDto?> GetUserPositionAsync(Guid userId)
    {
        var ranking = await GetRankingAsync();
        return ranking.FirstOrDefault(r => r.UserId == userId);
    }

    public void InvalidateCache()
    {
        cache.Remove(RANKING_CACHE_KEY);
    }

    public async Task<List<UserRankingsByGroupDto>> GetUserRankingsByGroupAsync(Guid userId)
    {
        var userGroups = await context.BolaoGroupMembers
            .Include(m => m.Group)
            .Where(m => m.UserId == userId && m.Status == MemberStatus.Active)
            .Select(m => m.Group)
            .ToListAsync();

        var result = new List<UserRankingsByGroupDto>();

        foreach (var group in userGroups)
        {
            var ranking = await context.Users
                .Where(u => u.IsActive && u.Predictions.Any(p => p.GroupId == group.Id && p.IsProcessed))
                .Select(u => new
                {
                    u.Id,
                    u.Name,
                    u.PhotoUrl,
                    TotalPoints = u.Predictions.Where(p => p.IsProcessed && p.GroupId == group.Id).Sum(p => p.Points),
                    ExactScores = u.Predictions.Count(p => p.IsProcessed && p.GroupId == group.Id && p.Points == 3),
                    CorrectOutcomes = u.Predictions.Count(p => p.IsProcessed && p.GroupId == group.Id && p.Points == 1),
                    TotalPredictions = u.Predictions.Count(p => p.GroupId == group.Id),
                    Errors = u.Predictions.Count(p => p.IsProcessed && p.GroupId == group.Id && p.Points == 0)
                })
                .OrderByDescending(u => u.TotalPoints)
                .ThenByDescending(u => u.ExactScores)
                .ThenByDescending(u => u.CorrectOutcomes)
                .ThenBy(u => u.Name)
                .ToListAsync();

            var entries = ranking.Select((entry, index) => new RankingEntryDto(
                Position: index + 1,
                UserId: entry.Id,
                UserName: entry.Name,
                UserPhotoUrl: entry.PhotoUrl,
                TotalPoints: entry.TotalPoints,
                ExactScores: entry.ExactScores,
                CorrectOutcomes: entry.CorrectOutcomes,
                TotalPredictions: entry.TotalPredictions,
                Errors: entry.Errors
            )).ToList();

            result.Add(new UserRankingsByGroupDto(
                GroupId: group.Id,
                GroupName: group.Name,
                GroupDescription: group.Description,
                TotalMembers: await context.BolaoGroupMembers.CountAsync(m => m.GroupId == group.Id && m.Status == MemberStatus.Active),
                Rankings: entries
            ));
        }

        return result;
    }
}
