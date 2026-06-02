using BolaoCopaMundo.Application.DTOs.Match;
using BolaoCopaMundo.Domain.Enums;
using BolaoCopaMundo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BolaoCopaMundo.Application.Services;

public class MatchService(AppDbContext context)
{
    public async Task<List<GroupDto>> GetAllGroupsAsync()
    {
        var groups = await context.Groups
            .Include(g => g.Teams)
            .Include(g => g.Matches)
                .ThenInclude(m => m.HomeTeam)
            .Include(g => g.Matches)
                .ThenInclude(m => m.AwayTeam)
            .OrderBy(g => g.Name)
            .ToListAsync();

        return groups.Select(ToGroupDto).ToList();
    }

    public async Task<GroupDto> GetGroupAsync(string name)
    {
        var group = await context.Groups
            .Include(g => g.Teams)
            .Include(g => g.Matches)
                .ThenInclude(m => m.HomeTeam)
            .Include(g => g.Matches)
                .ThenInclude(m => m.AwayTeam)
            .FirstOrDefaultAsync(g => g.Name == name.ToUpper())
            ?? throw new KeyNotFoundException($"Grupo '{name}' não encontrado.");

        return ToGroupDto(group);
    }

    public async Task<MatchDto> GetMatchAsync(int id)
    {
        var match = await context.Matches
            .Include(m => m.HomeTeam)
            .Include(m => m.AwayTeam)
            .Include(m => m.Group)
            .FirstOrDefaultAsync(m => m.Id == id)
            ?? throw new KeyNotFoundException("Jogo não encontrado.");

        return ToMatchDto(match);
    }

    public async Task<List<MatchDto>> GetUpcomingAsync(int hours = 24)
    {
        var cutoff = DateTime.UtcNow.AddHours(hours);
        return await context.Matches
            .Include(m => m.HomeTeam)
            .Include(m => m.AwayTeam)
            .Include(m => m.Group)
            .Where(m => m.Status == MatchStatus.Scheduled && m.MatchDate <= cutoff && m.MatchDate >= DateTime.UtcNow)
            .OrderBy(m => m.MatchDate)
            .Select(m => ToMatchDto(m))
            .ToListAsync();
    }

    public async Task<List<MatchDto>> GetByPhaseAsync(MatchPhase phase)
    {
        return await context.Matches
            .Include(m => m.HomeTeam)
            .Include(m => m.AwayTeam)
            .Include(m => m.Group)
            .Where(m => m.Phase == phase)
            .OrderBy(m => m.MatchDate)
            .Select(m => ToMatchDto(m))
            .ToListAsync();
    }

    private static GroupDto ToGroupDto(Domain.Entities.Group g) => new(
        g.Name,
        g.Teams.Select(t => new TeamDto(t.Id, t.Name, t.FifaCode, t.FlagUrl)).ToList(),
        g.Matches.OrderBy(m => m.Matchday).ThenBy(m => m.MatchDate).Select(ToMatchDto).ToList()
    );

    // MatchDate armazenado em UTC; exibe em BRT (UTC-3) para o público brasileiro
    private static readonly TimeZoneInfo Brt =
        TimeZoneInfo.FindSystemTimeZoneById(
            OperatingSystem.IsWindows() ? "E. South America Standard Time" : "America/Sao_Paulo");

    internal static MatchDto ToMatchDto(Domain.Entities.Match m) => new(
        m.Id,
        m.HomeTeam is null ? null : new TeamDto(m.HomeTeam.Id, m.HomeTeam.Name, m.HomeTeam.FifaCode, m.HomeTeam.FlagUrl),
        m.AwayTeam is null ? null : new TeamDto(m.AwayTeam.Id, m.AwayTeam.Name, m.AwayTeam.FifaCode, m.AwayTeam.FlagUrl),
        m.Group?.Name,
        m.Phase,
        m.Status,
        TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(m.MatchDate, DateTimeKind.Utc), Brt),
        m.HomeScore,
        m.AwayScore,
        m.Venue,
        m.MatchLabel,
        m.Matchday
    );
}
