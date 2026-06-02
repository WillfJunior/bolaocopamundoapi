using System.Security.Claims;
using BolaoCopaMundo.Application.DTOs.Prediction;
using BolaoCopaMundo.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BolaoCopaMundo.Controllers;

[ApiController]
[Route("api/predictions")]
[Authorize]
public class PredictionsController(PredictionService predictionService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<PredictionDto>>> GetMyPredictions([FromQuery] Guid groupId)
    {
        return Ok(await predictionService.GetUserPredictionsAsync(GetUserId(), groupId));
    }

    [HttpGet("match/{matchId:int}")]
    public async Task<ActionResult<PredictionDto?>> GetForMatch(int matchId, [FromQuery] Guid groupId)
    {
        return Ok(await predictionService.GetUserPredictionForMatchAsync(GetUserId(), matchId, groupId));
    }

    [HttpPost]
    public async Task<ActionResult<PredictionDto>> Save([FromBody] SavePredictionRequest request)
    {
        var userId = GetUserId();
        var result = await predictionService.SavePredictionAsync(userId, request);
        return Ok(result);
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
