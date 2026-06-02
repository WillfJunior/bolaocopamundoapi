namespace BolaoCopaMundo.Application.DTOs.Prediction;

public record SavePredictionRequest(Guid GroupId, int MatchId, int HomeScore, int AwayScore);
