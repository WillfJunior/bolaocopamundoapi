using BolaoCopaMundo.Domain.Enums;

namespace BolaoCopaMundo.Application.DTOs.BolaoGroup;

public record GroupInviteInfoDto(
    Guid GroupId,
    string GroupName,
    string? Description,
    string CreatorName,
    int MemberCount,
    bool IsAlreadyMember,
    MemberStatus? CurrentStatus
);
