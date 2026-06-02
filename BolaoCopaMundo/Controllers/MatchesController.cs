using BolaoCopaMundo.Application.DTOs.Match;
using BolaoCopaMundo.Application.Services;
using BolaoCopaMundo.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BolaoCopaMundo.Controllers;

[ApiController]
[Route("api/matches")]
[Authorize]
public class MatchesController(MatchService matchService) : ControllerBase
{
    [HttpGet("{id:int}")]
    public async Task<ActionResult<MatchDto>> GetMatch(int id)
        => Ok(await matchService.GetMatchAsync(id));

    [HttpGet("upcoming")]
    public async Task<ActionResult<List<MatchDto>>> GetUpcoming([FromQuery] int hours = 24)
        => Ok(await matchService.GetUpcomingAsync(hours));

    [HttpGet("phase/{phase}")]
    public async Task<ActionResult<List<MatchDto>>> GetByPhase(MatchPhase phase)
        => Ok(await matchService.GetByPhaseAsync(phase));
}
