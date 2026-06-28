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

    private static readonly DateTime[] Round16Dates =
    [
        new(2026, 7, 21, 16, 0, 0, DateTimeKind.Utc),
        new(2026, 7, 21, 20, 0, 0, DateTimeKind.Utc),
        new(2026, 7, 22, 16, 0, 0, DateTimeKind.Utc),
        new(2026, 7, 22, 20, 0, 0, DateTimeKind.Utc),
        new(2026, 7, 23, 16, 0, 0, DateTimeKind.Utc),
        new(2026, 7, 23, 20, 0, 0, DateTimeKind.Utc),
        new(2026, 7, 24, 16, 0, 0, DateTimeKind.Utc),
        new(2026, 7, 24, 20, 0, 0, DateTimeKind.Utc),
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

    public async Task GenerateRoundOf16Async()
    {
        // Verifica se todos os jogos das oitavas terminaram
        var pendingRound32Matches = await context.Matches
            .AnyAsync(m => m.Phase == MatchPhase.RoundOf32 && m.Status != MatchStatus.Finished);

        if (pendingRound32Matches)
        {
            logger.LogWarning("Oitavas de final não concluídas. 16 avos não gerados.");
            return;
        }

        var alreadyGenerated = await context.Matches.AnyAsync(m => m.Phase == MatchPhase.RoundOf16);
        if (alreadyGenerated)
        {
            logger.LogInformation("16 avos de final já gerados.");
            return;
        }

        // Obtém todos os jogos das oitavas ordenados por ID
        var round32Matches = await context.Matches
            .Include(m => m.HomeTeam)
            .Include(m => m.AwayTeam)
            .Where(m => m.Phase == MatchPhase.RoundOf32 && m.Status == MatchStatus.Finished)
            .OrderBy(m => m.Id)
            .ToListAsync();

        if (round32Matches.Count < 16)
        {
            logger.LogWarning("Menos de 16 jogos das oitavas encontrados.");
            return;
        }

        var round16Matches = new List<Match>();
        var dateIndex = 0;

        // Agrupa os jogos em pares (1vs2, 3vs4, 5vs6, 7vs8, 9vs10, 11vs12, 13vs14, 15vs16)
        // Cada dupla de jogos das oitavas gera um jogo das quartas (16 avos)
        for (int i = 0; i < 16; i += 2)
        {
            var match1 = round32Matches[i];
            var match2 = round32Matches[i + 1];

            // Determina os vencedores (HomeScore > AwayScore = vitória do time da casa)
            var winner1TeamId = match1.HomeScore > match1.AwayScore ? match1.HomeTeamId : match1.AwayTeamId;
            var winner2TeamId = match2.HomeScore > match2.AwayScore ? match2.HomeTeamId : match2.AwayTeamId;

            var winner1Name = match1.HomeScore > match1.AwayScore
                ? match1.HomeTeam?.Name ?? "?"
                : match1.AwayTeam?.Name ?? "?";
            var winner2Name = match2.HomeScore > match2.AwayScore
                ? match2.HomeTeam?.Name ?? "?"
                : match2.AwayTeam?.Name ?? "?";

            round16Matches.Add(new Match
            {
                HomeTeamId = winner1TeamId,
                AwayTeamId = winner2TeamId,
                Phase = MatchPhase.RoundOf16,
                Status = MatchStatus.Scheduled,
                Matchday = 1,
                MatchDate = Round16Dates[dateIndex % Round16Dates.Length],
                MatchLabel = $"{winner1Name} vs {winner2Name}",
                Venue = null
            });

            dateIndex++;
        }

        await context.Matches.AddRangeAsync(round16Matches);
        await context.SaveChangesAsync();

        logger.LogInformation("{Count} jogos dos 16 avos de final gerados.", round16Matches.Count);
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
