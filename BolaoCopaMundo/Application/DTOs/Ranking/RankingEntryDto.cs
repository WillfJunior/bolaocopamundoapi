namespace BolaoCopaMundo.Application.DTOs.Ranking;

public record RankingEntryDto(
    int Position,
    Guid UserId,
    string UserName,
    string? UserPhotoUrl,
    int TotalPoints,
    int ExactScores,
    int CorrectOutcomes,
    int TotalPredictions,
    int Errors
);
