namespace BolaoCopaMundo.Application.DTOs.Ranking;

public record UserRankingsByGroupDto(
    Guid GroupId,
    string GroupName,
    string? GroupDescription,
    int TotalMembers,
    List<RankingEntryDto> Rankings
);
