using System.Security.Claims;
using BolaoCopaMundo.Application.DTOs.User;
using BolaoCopaMundo.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BolaoCopaMundo.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController(UserService userService) : ControllerBase
{
    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> GetProfile()
    {
        var userId = GetUserId();
        return Ok(await userService.GetProfileAsync(userId));
    }

    [HttpPut("me")]
    public async Task<ActionResult<UserDto>> UpdateProfile([FromBody] UpdateUserRequest request)
    {
        var userId = GetUserId();
        return Ok(await userService.UpdateProfileAsync(userId, request));
    }

    [HttpPost("me/photo")]
    [RequestSizeLimit(1_048_576)]
    public async Task<IActionResult> UploadPhoto(IFormFile file)
    {
        var userId = GetUserId();
        var url = await userService.UploadPhotoAsync(userId, file);
        return Ok(new { photoUrl = url });
    }

    [HttpGet]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<List<UserDto>>> GetAll()
    {
        return Ok(await userService.GetAllAsync());
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
