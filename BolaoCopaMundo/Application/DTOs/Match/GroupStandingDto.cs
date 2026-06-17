namespace BolaoCopaMundo.Application.DTOs.Match;

public record GroupStandingDto(
    string GroupName,
    List<TeamStandingDto> Teams,
    List<MatchDto> Matches
);
