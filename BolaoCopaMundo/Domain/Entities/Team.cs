namespace BolaoCopaMundo.Domain.Entities;

public class Team
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string FifaCode { get; set; } = string.Empty;
    public string? FlagUrl { get; set; }
    public int? GroupId { get; set; }

    public Group? Group { get; set; }
    public ICollection<Match> HomeMatches { get; set; } = new List<Match>();
    public ICollection<Match> AwayMatches { get; set; } = new List<Match>();
}
