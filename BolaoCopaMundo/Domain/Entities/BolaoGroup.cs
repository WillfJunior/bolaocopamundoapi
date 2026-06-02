namespace BolaoCopaMundo.Domain.Entities;

public class BolaoGroup
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid CreatorId { get; set; }
    public string InviteCode { get; set; } = string.Empty;
    public string? PixKey { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    public User Creator { get; set; } = null!;
    public ICollection<BolaoGroupMember> Members { get; set; } = new List<BolaoGroupMember>();
}
