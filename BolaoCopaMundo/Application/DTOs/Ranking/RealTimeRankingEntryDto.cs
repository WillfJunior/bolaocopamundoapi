namespace BolaoCopaMundo.Application.DTOs.Ranking;

public record RealTimeRankingEntryDto(
    int Position,
    Guid UserId,
    string UserName,
    string? UserPhotoUrl,
    int TotalPoints,
    int ExactScores,
    int CorrectOutcomes,
    int TotalPredictions,
    int Errors,
    int MomentaryPoints,
    int MomentaryPosition,
    int PositionChange,
    bool IsLeader,
    int PointsDifference,
    DateTime UpdatedAt
);
