namespace BolaoCopaMundo.Application.DTOs.Match;

public record TeamStandingDto(
    int TeamId,
    string TeamName,
    string FifaCode,
    string? FlagUrl,
    int Position,
    int Played,
    int Won,
    int Drawn,
    int Lost,
    int GoalsFor,
    int GoalsAgainst,
    int GoalDifference,
    int Points
);
