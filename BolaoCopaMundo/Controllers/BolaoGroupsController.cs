using System.Security.Claims;
using BolaoCopaMundo.Application.DTOs.BolaoGroup;
using BolaoCopaMundo.Application.DTOs.Ranking;
using BolaoCopaMundo.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BolaoCopaMundo.Controllers;

[ApiController]
[Route("api/bolao-groups")]
[Authorize]
public class BolaoGroupsController(BolaoGroupService service) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<BolaoGroupDto>> Create([FromBody] CreateBolaoGroupRequest request)
    {
        var result = await service.CreateAsync(GetUserId(), request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet]
    public async Task<ActionResult<List<BolaoGroupDto>>> GetMine()
        => Ok(await service.GetMyGroupsAsync(GetUserId()));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BolaoGroupDto>> GetById(Guid id)
        => Ok(await service.GetByIdAsync(id, GetUserId()));

    /// <summary>Informações públicas do grupo pelo código de convite (não requer grupo ativo)</summary>
    [HttpGet("invite/{code}")]
    [AllowAnonymous]
    public async Task<ActionResult<GroupInviteInfoDto>> GetInviteInfo(string code)
    {
        Guid? userId = User.Identity?.IsAuthenticated == true ? GetUserId() : null;
        return Ok(await service.GetInviteInfoAsync(code, userId));
    }

    [HttpPost("invite/{code}/accept")]
    public async Task<ActionResult<BolaoGroupDto>> Accept(string code)
        => Ok(await service.AcceptInviteAsync(code, GetUserId()));

    [HttpPost("invite/{code}/reject")]
    public async Task<IActionResult> Reject(string code)
    {
        await service.RejectInviteAsync(code, GetUserId());
        return NoContent();
    }

    [HttpGet("{id:guid}/members")]
    public async Task<ActionResult<List<BolaoGroupMemberDto>>> GetMembers(Guid id)
        => Ok(await service.GetMembersAsync(id, GetUserId()));

    [HttpGet("{id:guid}/ranking")]
    public async Task<ActionResult<List<RankingEntryDto>>> GetRanking(Guid id)
        => Ok(await service.GetGroupRankingAsync(id, GetUserId()));

    [HttpDelete("{id:guid}/members/{userId:guid}")]
    public async Task<IActionResult> RemoveMember(Guid id, Guid userId)
    {
        await service.RemoveMemberAsync(id, GetUserId(), userId);
        return NoContent();
    }

    [HttpPost("{id:guid}/leave")]
    public async Task<IActionResult> Leave(Guid id)
    {
        await service.LeaveGroupAsync(id, GetUserId());
        return NoContent();
    }

    [HttpPost("{id:guid}/regenerate-invite")]
    public async Task<IActionResult> RegenerateInvite(Guid id)
    {
        var link = await service.RegenerateInviteCodeAsync(id, GetUserId());
        return Ok(new { inviteLink = link });
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
