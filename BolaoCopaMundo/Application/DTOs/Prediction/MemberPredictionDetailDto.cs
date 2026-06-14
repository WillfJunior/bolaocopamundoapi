using BolaoCopaMundo.Application.DTOs.Match;
using BolaoCopaMundo.Domain.Enums;

namespace BolaoCopaMundo.Application.DTOs.Prediction;

public record MemberPredictionDetailDto(
    Guid PredictionId,
    int MatchId,
    TeamDto? HomeTeam,
    TeamDto? AwayTeam,
    int PredictedHomeScore,
    int PredictedAwayScore,
    int? ActualHomeScore,
    int? ActualAwayScore,
    int Points,
    bool IsProcessed,
    MatchStatus MatchStatus,
    DateTime MatchDate,
    string? Venue,
    string? MatchLabel,
    int Matchday,
    MatchPhase Phase
);
