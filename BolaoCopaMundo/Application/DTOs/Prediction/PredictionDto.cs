namespace BolaoCopaMundo.Application.DTOs.Prediction;

public record PredictionDto(
    Guid Id,
    int MatchId,
    int HomeScore,
    int AwayScore,
    int Points,
    bool IsProcessed,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
