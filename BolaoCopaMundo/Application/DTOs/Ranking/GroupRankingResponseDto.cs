namespace BolaoCopaMundo.Application.DTOs.Ranking;

public record GroupRankingResponseDto(
    Guid GroupId,
    string GroupName,
    string? GroupDescription,
    int TotalMembers,
    int TotalMatches,
    int ProcessedMatches,
    string? CreatorName,
    List<GroupRankingEntryDto> Rankings,
    DateTime GeneratedAt
);

public record GroupRankingEntryDto(
    int Position,
    Guid UserId,
    string UserName,
    string? UserPhotoUrl,
    int TotalPoints,
    int ExactScores,
    int CorrectOutcomes,
    int TotalPredictions,
    double PointsPerPrediction,
    double AccuracyRate,
    bool IsLeader,
    int PointsDifference
);
