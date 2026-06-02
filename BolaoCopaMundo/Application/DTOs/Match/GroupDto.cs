namespace BolaoCopaMundo.Application.DTOs.Match;

public record GroupDto(string Name, List<TeamDto> Teams, List<MatchDto> Matches);
