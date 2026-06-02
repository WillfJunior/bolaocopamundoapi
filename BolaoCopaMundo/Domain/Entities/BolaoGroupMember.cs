using BolaoCopaMundo.Domain.Enums;

namespace BolaoCopaMundo.Domain.Entities;

public class BolaoGroupMember
{
    public Guid Id { get; set; }
    public Guid GroupId { get; set; }
    public Guid UserId { get; set; }
    public MemberRole Role { get; set; }
    public MemberStatus Status { get; set; }
    public DateTime InvitedAt { get; set; }
    public DateTime? JoinedAt { get; set; }

    public BolaoGroup Group { get; set; } = null!;
    public User User { get; set; } = null!;
}
