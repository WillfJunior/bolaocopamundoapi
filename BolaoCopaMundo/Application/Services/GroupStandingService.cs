using BolaoCopaMundo.Application.DTOs.Match;
using BolaoCopaMundo.Domain.Enums;
using BolaoCopaMundo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace BolaoCopaMundo.Application.Services;

public class GroupStandingService(AppDbContext context, IMemoryCache cache)
{
    private const string STANDINGS_CACHE_KEY_PREFIX = "standings:group:";
    private static readonly TimeSpan CACHE_DURATION = TimeSpan.FromSeconds(30);

    public async Task<GroupStandingDto> GetGroupStandingAsync(string groupName)
    {
        string cacheKey = $"{STANDINGS_CACHE_KEY_PREFIX}{groupName.ToUpper()}";
        if (cache.TryGetValue(cacheKey, out GroupStandingDto? cachedStanding))
            return cachedStanding!;

        var group = await context.Groups
            .Include(g => g.Teams)
            .Include(g => g.Matches)
                .ThenInclude(m => m.HomeTeam)
            .Include(g => g.Matches)
                .ThenInclude(m => m.AwayTeam)
            .FirstOrDefaultAsync(g => g.Name == groupName.ToUpper())
            ?? throw new KeyNotFoundException($"Grupo '{groupName}' não encontrado.");

        var standings = CalculateStandings(group);
        cache.Set(cacheKey, standings, CACHE_DURATION);
        return standings;
    }

    public async Task<List<GroupStandingDto>> GetAllGroupStandingsAsync()
    {
        var groups = await context.Groups
            .Include(g => g.Teams)
            .Include(g => g.Matches)
                .ThenInclude(m => m.HomeTeam)
            .Include(g => g.Matches)
                .ThenInclude(m => m.AwayTeam)
            .OrderBy(g => g.Name)
            .ToListAsync();

        return groups.Select(CalculateStandings).ToList();
    }

    public void InvalidateGroupCache(string groupName)
    {
        cache.Remove($"{STANDINGS_CACHE_KEY_PREFIX}{groupName.ToUpper()}");
    }

    private GroupStandingDto CalculateStandings(Domain.Entities.Group group)
    {
        var teamStats = new Dictionary<int, TeamStats>();

        foreach (var team in group.Teams)
        {
            teamStats[team.Id] = new TeamStats
            {
                TeamId = team.Id,
                TeamName = team.Name,
                FifaCode = team.FifaCode,
                FlagUrl = team.FlagUrl
            };
        }

        var finishedMatches = group.Matches.Where(m => m.Status == MatchStatus.Finished).ToList();

        foreach (var match in finishedMatches)
        {
            if (match.HomeTeamId is null || match.AwayTeamId is null ||
                match.HomeScore is null || match.AwayScore is null)
                continue;

            var home = teamStats[match.HomeTeamId.Value];
            var away = teamStats[match.AwayTeamId.Value];

            home.Played++;
            away.Played++;
            home.GoalsFor += match.HomeScore.Value;
            home.GoalsAgainst += match.AwayScore.Value;
            away.GoalsFor += match.AwayScore.Value;
            away.GoalsAgainst += match.HomeScore.Value;

            if (match.HomeScore > match.AwayScore)
            {
                home.Won++;
                home.Points += 3;
                away.Lost++;
            }
            else if (match.HomeScore < match.AwayScore)
            {
                away.Won++;
                away.Points += 3;
                home.Lost++;
            }
            else
            {
                home.Drawn++;
                home.Points += 1;
                away.Drawn++;
                away.Points += 1;
            }
        }

        var sortedTeams = SortTeams(teamStats.Values.ToList());

        var teamDtos = sortedTeams.Select((team, index) => new TeamStandingDto(
            team.TeamId,
            team.TeamName,
            team.FifaCode,
            team.FlagUrl,
            index + 1,
            team.Played,
            team.Won,
            team.Drawn,
            team.Lost,
            team.GoalsFor,
            team.GoalsAgainst,
            team.GoalsFor - team.GoalsAgainst,
            team.Points
        )).ToList();

        var matches = group.Matches
            .OrderBy(m => m.MatchDate)
            .Select(MatchService.ToMatchDto)
            .ToList();

        return new GroupStandingDto(group.Name, teamDtos, matches);
    }

    private List<TeamStats> SortTeams(List<TeamStats> teams)
    {
        return teams
            .OrderByDescending(t => t.Points)
            .ThenByDescending(t => t.GoalsFor - t.GoalsAgainst)
            .ThenByDescending(t => t.GoalsFor)
            .ThenBy(t => t.TeamName)
            .ToList();
    }

    private class TeamStats
    {
        public int TeamId { get; set; }
        public string TeamName { get; set; } = string.Empty;
        public string FifaCode { get; set; } = string.Empty;
        public string? FlagUrl { get; set; }
        public int Played { get; set; }
        public int Won { get; set; }
        public int Drawn { get; set; }
        public int Lost { get; set; }
        public int GoalsFor { get; set; }
        public int GoalsAgainst { get; set; }
        public int Points { get; set; }
    }
}
