using BolaoCopaMundo.Domain.Enums;

namespace BolaoCopaMundo.Domain.Entities;

public class Match
{
    public int Id { get; set; }
    public int? HomeTeamId { get; set; }
    public int? AwayTeamId { get; set; }
    public int? GroupId { get; set; }
    public MatchPhase Phase { get; set; }
    public MatchStatus Status { get; set; } = MatchStatus.Scheduled;
    public DateTime MatchDate { get; set; }
    public int? HomeScore { get; set; }
    public int? AwayScore { get; set; }
    public string? ExternalId { get; set; }
    public string? Venue { get; set; }
    public string? MatchLabel { get; set; }
    public int Matchday { get; set; }

    public Team? HomeTeam { get; set; }
    public Team? AwayTeam { get; set; }
    public Group? Group { get; set; }
    public ICollection<Prediction> Predictions { get; set; } = new List<Prediction>();
}
