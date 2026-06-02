using BolaoCopaMundo.Application.DTOs.BolaoGroup;
using BolaoCopaMundo.Application.DTOs.Ranking;
using BolaoCopaMundo.Domain.Entities;
using BolaoCopaMundo.Domain.Enums;
using BolaoCopaMundo.Infrastructure.Data;
using BolaoCopaMundo.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace BolaoCopaMundo.Application.Services;

public class BolaoGroupService(AppDbContext context, IConfiguration configuration, PushNotificationService pushService)
{
    private string AppBaseUrl =>
        configuration["AppBaseUrl"] ?? "http://localhost:5196";

    public async Task<BolaoGroupDto> CreateAsync(Guid creatorId, CreateBolaoGroupRequest request)
    {
        var creator = await context.Users.FindAsync(creatorId)
            ?? throw new KeyNotFoundException("Usuário não encontrado.");

        if (!creator.IsAdmin)
            throw new UnauthorizedAccessException("Apenas administradores podem criar grupos.");

        var inviteCode = GenerateInviteCode();
        while (await context.BolaoGroups.AnyAsync(g => g.InviteCode == inviteCode))
            inviteCode = GenerateInviteCode();

        var group = new BolaoGroup
        {
            Name = request.Name,
            Description = request.Description,
            PixKey = request.PixKey,
            CreatorId = creatorId,
            InviteCode = inviteCode,
            CreatedAt = DateTime.UtcNow
        };

        context.BolaoGroups.Add(group);
        await context.SaveChangesAsync();

        context.BolaoGroupMembers.Add(new BolaoGroupMember
        {
            GroupId = group.Id,
            UserId = creatorId,
            Role = MemberRole.Admin,
            Status = MemberStatus.Active,
            InvitedAt = DateTime.UtcNow,
            JoinedAt = DateTime.UtcNow
        });

        await context.SaveChangesAsync();
        return ToDto(group, creator, MemberRole.Admin, MemberStatus.Active, 1, 0);
    }

    public async Task<List<BolaoGroupDto>> GetMyGroupsAsync(Guid userId)
    {
        var memberships = await context.BolaoGroupMembers
            .Include(m => m.Group).ThenInclude(g => g.Creator)
            .Include(m => m.Group).ThenInclude(g => g.Members)
            .Where(m => m.UserId == userId && m.Group.IsActive)
            .OrderByDescending(m => m.JoinedAt)
            .ToListAsync();

        return memberships.Select(m => ToDto(
            m.Group,
            m.Group.Creator,
            m.Role,
            m.Status,
            m.Group.Members.Count(x => x.Status == MemberStatus.Active),
            m.Group.Members.Count(x => x.Status == MemberStatus.Pending)
        )).ToList();
    }

    public async Task<BolaoGroupDto> GetByIdAsync(Guid groupId, Guid userId)
    {
        var group = await context.BolaoGroups
            .Include(g => g.Creator)
            .Include(g => g.Members)
            .FirstOrDefaultAsync(g => g.Id == groupId && g.IsActive)
            ?? throw new KeyNotFoundException("Grupo não encontrado.");

        var membership = group.Members.FirstOrDefault(m => m.UserId == userId)
            ?? throw new UnauthorizedAccessException("Você não faz parte deste grupo.");

        return ToDto(group, group.Creator, membership.Role, membership.Status,
            group.Members.Count(m => m.Status == MemberStatus.Active),
            group.Members.Count(m => m.Status == MemberStatus.Pending));
    }

    public async Task<GroupInviteInfoDto> GetInviteInfoAsync(string inviteCode, Guid? userId)
    {
        var group = await context.BolaoGroups
            .Include(g => g.Creator)
            .Include(g => g.Members)
            .FirstOrDefaultAsync(g => g.InviteCode == inviteCode && g.IsActive)
            ?? throw new KeyNotFoundException("Convite inválido ou expirado.");

        BolaoGroupMember? membership = null;
        if (userId.HasValue)
            membership = group.Members.FirstOrDefault(m => m.UserId == userId.Value);

        return new GroupInviteInfoDto(
            group.Id,
            group.InviteCode,
            group.Name,
            group.Description,
            group.PixKey,
            group.Creator.Name,
            group.Members.Count(m => m.Status == MemberStatus.Active),
            membership?.Status == MemberStatus.Active,
            membership?.Status
        );
    }

    public async Task<BolaoGroupDto> AcceptInviteAsync(string inviteCode, Guid userId)
    {
        var group = await context.BolaoGroups
            .Include(g => g.Creator)
            .Include(g => g.Members)
            .FirstOrDefaultAsync(g => g.InviteCode == inviteCode && g.IsActive)
            ?? throw new KeyNotFoundException("Convite inválido ou expirado.");

        var existing = group.Members.FirstOrDefault(m => m.UserId == userId);

        if (existing is not null)
        {
            if (existing.Status == MemberStatus.Active)
                throw new InvalidOperationException("Você já é membro deste grupo.");
            if (existing.Status == MemberStatus.Pending)
                throw new InvalidOperationException("Sua solicitação já está aguardando aprovação do administrador.");

            existing.Status = MemberStatus.Pending;
            existing.JoinedAt = null;
        }
        else
        {
            context.BolaoGroupMembers.Add(new BolaoGroupMember
            {
                GroupId = group.Id,
                UserId = userId,
                Role = MemberRole.Member,
                Status = MemberStatus.Pending,
                InvitedAt = DateTime.UtcNow
            });
        }

        await context.SaveChangesAsync();

        var pendingCount = group.Members.Count(m => m.Status == MemberStatus.Pending) + (existing is null ? 1 : 0);
        await pushService.SendToUserAsync(
            group.CreatorId,
            "Bolão Copa 2026 ⚽",
            $"{pendingCount} pessoa(s) aguardando aprovação no grupo \"{group.Name}\".");

        return ToDto(group, group.Creator, MemberRole.Member, MemberStatus.Pending,
            group.Members.Count(m => m.Status == MemberStatus.Active), pendingCount);
    }

    public async Task RejectInviteAsync(string inviteCode, Guid userId)
    {
        var group = await context.BolaoGroups
            .Include(g => g.Members)
            .FirstOrDefaultAsync(g => g.InviteCode == inviteCode && g.IsActive)
            ?? throw new KeyNotFoundException("Convite inválido ou expirado.");

        var existing = group.Members.FirstOrDefault(m => m.UserId == userId);

        if (existing is not null)
        {
            if (existing.Status == MemberStatus.Active)
                throw new InvalidOperationException("Você já é membro ativo. Use 'Sair do grupo' para sair.");
            existing.Status = MemberStatus.Rejected;
        }
        else
        {
            context.BolaoGroupMembers.Add(new BolaoGroupMember
            {
                GroupId = group.Id,
                UserId = userId,
                Role = MemberRole.Member,
                Status = MemberStatus.Rejected,
                InvitedAt = DateTime.UtcNow
            });
        }

        await context.SaveChangesAsync();
    }

    public async Task<List<BolaoGroupMemberDto>> GetPendingMembersAsync(Guid groupId, Guid adminId)
    {
        await EnsureAdminAsync(groupId, adminId);

        return await context.BolaoGroupMembers
            .Include(m => m.User)
            .Where(m => m.GroupId == groupId && m.Status == MemberStatus.Pending)
            .OrderBy(m => m.InvitedAt)
            .Select(m => new BolaoGroupMemberDto(
                m.UserId, m.User.Name, m.User.PhotoUrl,
                m.Role, m.Status, m.InvitedAt, m.JoinedAt))
            .ToListAsync();
    }

    public async Task<BolaoGroupMemberDto> ApproveMemberAsync(Guid groupId, Guid adminId, Guid targetUserId)
    {
        await EnsureAdminAsync(groupId, adminId);

        var member = await context.BolaoGroupMembers
            .Include(m => m.User)
            .FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == targetUserId
                                      && m.Status == MemberStatus.Pending)
            ?? throw new KeyNotFoundException("Solicitação pendente não encontrada.");

        member.Status = MemberStatus.Active;
        member.JoinedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();

        await pushService.SendToUserAsync(
            targetUserId,
            "Bolão Copa 2026 ⚽",
            "Sua entrada foi aprovada! Agora é só fazer seus palpites e torcer. Bora ganhar! 🏆");

        return new BolaoGroupMemberDto(
            member.UserId, member.User.Name, member.User.PhotoUrl,
            member.Role, member.Status, member.InvitedAt, member.JoinedAt);
    }

    public async Task<List<BolaoGroupMemberDto>> GetMembersAsync(Guid groupId, Guid userId)
    {
        await EnsureMemberAsync(groupId, userId);

        return await context.BolaoGroupMembers
            .Include(m => m.User)
            .Where(m => m.GroupId == groupId)
            .OrderBy(m => m.Role).ThenBy(m => m.User.Name)
            .Select(m => new BolaoGroupMemberDto(
                m.UserId, m.User.Name, m.User.PhotoUrl,
                m.Role, m.Status, m.InvitedAt, m.JoinedAt))
            .ToListAsync();
    }

    public async Task<List<RankingEntryDto>> GetGroupRankingAsync(Guid groupId, Guid userId)
    {
        await EnsureMemberAsync(groupId, userId);

        var memberIds = await context.BolaoGroupMembers
            .Where(m => m.GroupId == groupId && m.Status == MemberStatus.Active)
            .Select(m => m.UserId)
            .ToListAsync();

        var raw = await context.Users
            .Where(u => memberIds.Contains(u.Id) && u.IsActive)
            .Select(u => new
            {
                u.Id, u.Name, u.PhotoUrl,
                TotalPoints      = u.Predictions.Where(p => p.IsProcessed && p.GroupId == groupId).Sum(p => p.Points),
                ExactScores      = u.Predictions.Count(p => p.IsProcessed && p.GroupId == groupId && p.Points == 3),
                CorrectOutcomes  = u.Predictions.Count(p => p.IsProcessed && p.GroupId == groupId && p.Points == 1),
                TotalPredictions = u.Predictions.Count(p => p.GroupId == groupId)
            })
            .OrderByDescending(u => u.TotalPoints)
            .ThenByDescending(u => u.ExactScores)
            .ThenByDescending(u => u.CorrectOutcomes)
            .ThenBy(u => u.Name)
            .ToListAsync();

        return raw.Select((e, i) => new RankingEntryDto(
            i + 1, e.Id, e.Name, e.PhotoUrl,
            e.TotalPoints, e.ExactScores, e.CorrectOutcomes, e.TotalPredictions
        )).ToList();
    }

    public async Task RemoveMemberAsync(Guid groupId, Guid adminId, Guid targetUserId)
    {
        await EnsureAdminAsync(groupId, adminId);

        if (adminId == targetUserId)
            throw new InvalidOperationException("O criador não pode ser removido. Exclua o grupo.");

        var member = await context.BolaoGroupMembers
            .FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == targetUserId)
            ?? throw new KeyNotFoundException("Membro não encontrado.");

        context.BolaoGroupMembers.Remove(member);
        await context.SaveChangesAsync();
    }

    public async Task LeaveGroupAsync(Guid groupId, Guid userId)
    {
        var group = await context.BolaoGroups
            .Include(g => g.Members)
            .FirstOrDefaultAsync(g => g.Id == groupId)
            ?? throw new KeyNotFoundException("Grupo não encontrado.");

        if (group.CreatorId == userId)
            throw new InvalidOperationException("O criador não pode sair do grupo. Exclua o grupo.");

        var member = group.Members.FirstOrDefault(m => m.UserId == userId)
            ?? throw new KeyNotFoundException("Você não faz parte deste grupo.");

        context.BolaoGroupMembers.Remove(member);
        await context.SaveChangesAsync();
    }

    public async Task<string> RegenerateInviteCodeAsync(Guid groupId, Guid adminId)
    {
        await EnsureAdminAsync(groupId, adminId);

        var group = await context.BolaoGroups.FindAsync(groupId)!;

        var newCode = GenerateInviteCode();
        while (await context.BolaoGroups.AnyAsync(g => g.InviteCode == newCode))
            newCode = GenerateInviteCode();

        group!.InviteCode = newCode;
        await context.SaveChangesAsync();

        return BuildInviteLink(newCode);
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private async Task EnsureMemberAsync(Guid groupId, Guid userId)
    {
        var isMember = await context.BolaoGroupMembers.AnyAsync(m =>
            m.GroupId == groupId && m.UserId == userId && m.Status == MemberStatus.Active);

        if (!isMember) throw new UnauthorizedAccessException("Você não faz parte deste grupo.");
    }

    private async Task EnsureAdminAsync(Guid groupId, Guid userId)
    {
        var isAdmin = await context.BolaoGroupMembers.AnyAsync(m =>
            m.GroupId == groupId && m.UserId == userId &&
            m.Role == MemberRole.Admin && m.Status == MemberStatus.Active);

        if (!isAdmin) throw new UnauthorizedAccessException("Apenas administradores podem realizar esta ação.");
    }

    private string BuildInviteLink(string code) => $"{AppBaseUrl}/join/{code}";

    private string BuildWhatsAppUrl(string groupName, string inviteLink)
    {
        var text = Uri.EscapeDataString(
            $"🏆⚽ Participe do meu Bolão da Copa 2026!\n" +
            $"Grupo: *{groupName}*\n\n" +
            $"Clique no link para entrar:\n{inviteLink}");
        return $"https://wa.me/?text={text}";
    }

    private BolaoGroupDto ToDto(BolaoGroup g, User creator, MemberRole role, MemberStatus status, int memberCount, int pendingCount)
    {
        var link = BuildInviteLink(g.InviteCode);
        return new BolaoGroupDto(
            g.Id, g.Name, g.Description, g.PixKey,
            g.CreatorId, creator.Name,
            g.InviteCode, link,
            BuildWhatsAppUrl(g.Name, link),
            memberCount, pendingCount, role, status, g.CreatedAt);
    }

    private static string GenerateInviteCode()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        return new string(Enumerable.Range(0, 8)
            .Select(_ => chars[Random.Shared.Next(chars.Length)])
            .ToArray());
    }
}
