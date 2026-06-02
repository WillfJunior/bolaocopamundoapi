using BolaoCopaMundo.Domain.Enums;

namespace BolaoCopaMundo.Application.DTOs.Match;

public record MatchDto(
    int Id,
    TeamDto? HomeTeam,
    TeamDto? AwayTeam,
    string? GroupName,
    MatchPhase Phase,
    MatchStatus Status,
    DateTime MatchDate,
    int? HomeScore,
    int? AwayScore,
    string? Venue,
    string? MatchLabel,
    int Matchday
);
