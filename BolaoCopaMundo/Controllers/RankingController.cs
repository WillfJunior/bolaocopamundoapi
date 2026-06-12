using System.Security.Claims;
using BolaoCopaMundo.Application.DTOs.Ranking;
using BolaoCopaMundo.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BolaoCopaMundo.Controllers;

[ApiController]
[Route("api/ranking")]
[Authorize]
public class RankingController(RankingService rankingService, BolaoGroupService bolaoGroupService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<object>> GetRanking([FromQuery] Guid? groupId)
    {
        // Se não tiver groupId, retorna ranking global
        if (!groupId.HasValue)
            return Ok(await rankingService.GetRankingAsync());

        // Se tiver groupId, retorna ranking detalhado do grupo
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Ok(await bolaoGroupService.GetGroupRankingDetailedAsync(groupId.Value, userId));
    }

    [HttpGet("me")]
    public async Task<ActionResult<RankingEntryDto?>> GetMyPosition()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Ok(await rankingService.GetUserPositionAsync(userId));
    }
}
