namespace BolaoCopaMundo.Application.DTOs.Prediction;

public record MemberPredictionsResponseDto(
    Guid MemberId,
    string MemberName,
    string? MemberPhotoUrl,
    Guid GroupId,
    int NextMatchId,
    DateTime NextMatchDate,
    DateTime PredictionDeadline,
    bool CanViewPredictions,
    MemberPredictionDetailDto Prediction
);
