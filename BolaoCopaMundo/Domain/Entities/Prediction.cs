namespace BolaoCopaMundo.Domain.Entities;

public class Prediction
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public int MatchId { get; set; }
    public int HomeScore { get; set; }
    public int AwayScore { get; set; }
    public int Points { get; set; }
    public bool IsProcessed { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public User User { get; set; } = null!;
    public Match Match { get; set; } = null!;
}
