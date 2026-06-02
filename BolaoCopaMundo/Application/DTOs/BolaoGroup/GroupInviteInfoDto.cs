using BolaoCopaMundo.Domain.Enums;

namespace BolaoCopaMundo.Application.DTOs.BolaoGroup;

public record GroupInviteInfoDto(
    Guid GroupId,
    string InviteCode,
    string GroupName,
    string? Description,
    string? PixKey,
    string CreatorName,
    int MemberCount,
    bool IsAlreadyMember,
    MemberStatus? CurrentStatus
);
