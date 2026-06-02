using BolaoCopaMundo.Application.DTOs.Match;
using BolaoCopaMundo.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BolaoCopaMundo.Controllers;

[ApiController]
[Route("api/groups")]
[Authorize]
public class GroupsController(MatchService matchService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<GroupDto>>> GetAll()
        => Ok(await matchService.GetAllGroupsAsync());

    [HttpGet("{name}")]
    public async Task<ActionResult<GroupDto>> GetGroup(string name)
        => Ok(await matchService.GetGroupAsync(name));
}
