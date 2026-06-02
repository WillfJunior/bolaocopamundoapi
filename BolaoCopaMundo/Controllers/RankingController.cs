using System.Security.Claims;
using BolaoCopaMundo.Application.DTOs.Ranking;
using BolaoCopaMundo.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BolaoCopaMundo.Controllers;

[ApiController]
[Route("api/ranking")]
[Authorize]
public class RankingController(RankingService rankingService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<RankingEntryDto>>> GetRanking()
        => Ok(await rankingService.GetRankingAsync());

    [HttpGet("me")]
    public async Task<ActionResult<RankingEntryDto?>> GetMyPosition()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Ok(await rankingService.GetUserPositionAsync(userId));
    }
}
