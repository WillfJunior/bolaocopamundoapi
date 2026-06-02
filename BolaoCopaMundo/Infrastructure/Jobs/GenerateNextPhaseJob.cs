using BolaoCopaMundo.Domain.Entities;
using BolaoCopaMundo.Domain.Enums;
using BolaoCopaMundo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BolaoCopaMundo.Infrastructure.Jobs;

public class GenerateNextPhaseJob(AppDbContext context, ILogger<GenerateNextPhaseJob> logger)
{
    // Oitavas da Copa 2026: mapeamento oficial FIFA (baseado nos grupos)
    // 1ºA vs 2ºB, 1ºC vs 2ºD, ... mais 8 melhores terceiros colocados
    private static readonly (string Winner, string RunnerUp)[] Round32Fixtures =
    [
        ("A", "B"), ("C", "D"), ("E", "F"), ("G", "H"),
        ("I", "J"), ("K", "L"), ("A", "C"), ("B", "D"),
        ("E", "G"), ("F", "H"), ("I", "K"), ("J", "L"),
        // Os outros 4 jogos envolvem terceiros colocados — gerados dinamicamente
    ];

    private static readonly DateTime[] Round32Dates =
    [
        new(2026, 7, 12, 16, 0, 0, DateTimeKind.Utc),
        new(2026, 7, 12, 20, 0, 0, DateTimeKind.Utc),
        new(2026, 7, 13, 16, 0, 0, DateTimeKind.Utc),
        new(2026, 7, 13, 20, 0, 0, DateTimeKind.Utc),
        new(2026, 7, 14, 16, 0, 0, DateTimeKind.Utc),
        new(2026, 7, 14, 20, 0, 0, DateTimeKind.Utc),
        new(2026, 7, 15, 16, 0, 0, DateTimeKind.Utc),
        new(2026, 7, 15, 20, 0, 0, DateTimeKind.Utc),
        new(2026, 7, 16, 16, 0, 0, DateTimeKind.Utc),
        new(2026, 7, 16, 20, 0, 0, DateTimeKind.Utc),
        new(2026, 7, 17, 16, 0, 0, DateTimeKind.Utc),
        new(2026, 7, 17, 20, 0, 0, DateTimeKind.Utc),
        new(2026, 7, 18, 16, 0, 0, DateTimeKind.Utc),
        new(2026, 7, 18, 20, 0, 0, DateTimeKind.Utc),
        new(2026, 7, 19, 16, 0, 0, DateTimeKind.Utc),
        new(2026, 7, 19, 20, 0, 0, DateTimeKind.Utc),
    ];

    public async Task GenerateRoundOf32Async()
    {
        // Verifica se todos os jogos da fase de grupos terminaram
        var pendingGroupMatches = await context.Matches
            .AnyAsync(m => m.Phase == MatchPhase.GroupStage && m.Status != MatchStatus.Finished);

        if (pendingGroupMatches)
        {
            logger.LogWarning("Fase de grupos não concluída. Oitavas não geradas.");
            return;
        }

        var alreadyGenerated = await context.Matches.AnyAsync(m => m.Phase == MatchPhase.RoundOf32);
        if (alreadyGenerated)
        {
            logger.LogInformation("Oitavas já geradas.");
            return;
        }

        var standings = await GetGroupStandingsAsync();
        var matches = new List<Match>();
        var dateIndex = 0;

        // Gera 12 jogos: 1º vs 2º de grupos diferentes
        foreach (var (winnerGroup, runnerUpGroup) in Round32Fixtures)
        {
            var winner = standings.FirstOrDefault(s => s.GroupName == winnerGroup && s.Position == 1);
            var runnerUp = standings.FirstOrDefault(s => s.GroupName == runnerUpGroup && s.Position == 2);

            matches.Add(new Match
            {
                HomeTeamId = winner?.TeamId,
                AwayTeamId = runnerUp?.TeamId,
                Phase = MatchPhase.RoundOf32,
                Status = MatchStatus.Scheduled,
                Matchday = 1,
                MatchDate = Round32Dates[dateIndex % Round32Dates.Length],
                MatchLabel = winner is null
                    ? $"1º Grupo {winnerGroup} vs 2º Grupo {runnerUpGroup}"
                    : $"{winner.TeamName} vs {runnerUp?.TeamName ?? "2º " + runnerUpGroup}"
            });

            dateIndex++;
        }

        // 4 jogos extras com os melhores terceiros colocados
        var bestThirds = standings
            .Where(s => s.Position == 3)
            .OrderByDescending(s => s.Points)
            .ThenByDescending(s => s.GoalDifference)
            .ThenByDescending(s => s.GoalsFor)
            .Take(4)
            .ToList();

        for (int i = 0; i < bestThirds.Count / 2; i++)
        {
            matches.Add(new Match
            {
                HomeTeamId = bestThirds[i * 2].TeamId,
                AwayTeamId = bestThirds[i * 2 + 1].TeamId,
                Phase = MatchPhase.RoundOf32,
                Status = MatchStatus.Scheduled,
                Matchday = 1,
                MatchDate = Round32Dates[dateIndex % Round32Dates.Length],
                MatchLabel = $"Melhor 3º colocado ({i + 1})"
            });
            dateIndex++;
        }

        await context.Matches.AddRangeAsync(matches);
        await context.SaveChangesAsync();

        logger.LogInformation("{Count} jogos das oitavas de final gerados.", matches.Count);
    }

    private async Task<List<GroupStanding>> GetGroupStandingsAsync()
    {
        var groupMatches = await context.Matches
            .Include(m => m.HomeTeam)
            .Include(m => m.AwayTeam)
            .Include(m => m.Group)
            .Where(m => m.Phase == MatchPhase.GroupStage && m.Status == MatchStatus.Finished)
            .ToListAsync();

        var teams = await context.Teams
            .Include(t => t.Group)
            .Where(t => t.GroupId != null)
            .ToListAsync();

        var standings = new List<GroupStanding>();

        foreach (var team in teams)
        {
            var homeMatches = groupMatches.Where(m => m.HomeTeamId == team.Id).ToList();
            var awayMatches = groupMatches.Where(m => m.AwayTeamId == team.Id).ToList();

            int points = 0, gf = 0, ga = 0, wins = 0;

            foreach (var m in homeMatches)
            {
                if (m.HomeScore > m.AwayScore) { points += 3; wins++; }
                else if (m.HomeScore == m.AwayScore) points += 1;
                gf += m.HomeScore ?? 0;
                ga += m.AwayScore ?? 0;
            }

            foreach (var m in awayMatches)
            {
                if (m.AwayScore > m.HomeScore) { points += 3; wins++; }
                else if (m.AwayScore == m.HomeScore) points += 1;
                gf += m.AwayScore ?? 0;
                ga += m.HomeScore ?? 0;
            }

            standings.Add(new GroupStanding
            {
                TeamId = team.Id,
                TeamName = team.Name,
                GroupName = team.Group!.Name,
                Points = points,
                GoalsFor = gf,
                GoalDifference = gf - ga,
                Wins = wins
            });
        }

        // Ordena dentro de cada grupo para definir posições
        var groupedStandings = standings.GroupBy(s => s.GroupName);
        var result = new List<GroupStanding>();

        foreach (var grp in groupedStandings)
        {
            var ordered = grp.OrderByDescending(s => s.Points)
                             .ThenByDescending(s => s.GoalDifference)
                             .ThenByDescending(s => s.GoalsFor)
                             .ThenBy(s => s.TeamName)
                             .ToList();

            for (int i = 0; i < ordered.Count; i++)
                ordered[i].Position = i + 1;

            result.AddRange(ordered);
        }

        return result;
    }

    private class GroupStanding
    {
        public int TeamId { get; set; }
        public string TeamName { get; set; } = string.Empty;
        public string GroupName { get; set; } = string.Empty;
        public int Points { get; set; }
        public int GoalsFor { get; set; }
        public int GoalDifference { get; set; }
        public int Wins { get; set; }
        public int Position { get; set; }
    }
}
