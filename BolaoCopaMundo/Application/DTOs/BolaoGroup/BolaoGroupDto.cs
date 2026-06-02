using BolaoCopaMundo.Domain.Enums;

namespace BolaoCopaMundo.Application.DTOs.BolaoGroup;

public record BolaoGroupDto(
    Guid Id,
    string Name,
    string? Description,
    Guid CreatorId,
    string CreatorName,
    string InviteCode,
    string InviteLink,
    string WhatsAppShareUrl,
    int MemberCount,
    MemberRole MyRole,
    MemberStatus MyStatus,
    DateTime CreatedAt
);
