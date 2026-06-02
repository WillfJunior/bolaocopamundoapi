using BolaoCopaMundo.Domain.Enums;

namespace BolaoCopaMundo.Application.DTOs.BolaoGroup;

public record BolaoGroupMemberDto(
    Guid UserId,
    string UserName,
    string? UserPhotoUrl,
    MemberRole Role,
    MemberStatus Status,
    DateTime InvitedAt,
    DateTime? JoinedAt
);
