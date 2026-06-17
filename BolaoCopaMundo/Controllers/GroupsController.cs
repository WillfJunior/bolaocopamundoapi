using BolaoCopaMundo.Application.DTOs.Match;
using BolaoCopaMundo.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BolaoCopaMundo.Controllers;

[ApiController]
[Route("api/groups")]
[Authorize]
public class GroupsController(MatchService matchService, GroupStandingService standingService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<GroupDto>>> GetAll()
        => Ok(await matchService.GetAllGroupsAsync());

    [HttpGet("{name}")]
    public async Task<ActionResult<GroupDto>> GetGroup(string name)
        => Ok(await matchService.GetGroupAsync(name));

    [HttpGet("{name}/standings")]
    public async Task<ActionResult<GroupStandingDto>> GetGroupStanding(string name)
        => Ok(await standingService.GetGroupStandingAsync(name));

    [HttpGet("standings/all")]
    public async Task<ActionResult<List<GroupStandingDto>>> GetAllStandings()
        => Ok(await standingService.GetAllGroupStandingsAsync());
}
