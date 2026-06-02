using BolaoCopaMundo.Application.DTOs.Match;
using BolaoCopaMundo.Application.Services;
using BolaoCopaMundo.Domain.Enums;
using BolaoCopaMundo.Infrastructure.Data;
using BolaoCopaMundo.Infrastructure.Jobs;
using BolaoCopaMundo.Infrastructure.Services;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
#pragma warning disable CA2254

namespace BolaoCopaMundo.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Policy = "AdminOnly")]
public class AdminController(
    AppDbContext context,
    PushNotificationService pushService,
    GenerateNextPhaseJob nextPhaseJob) : ControllerBase
{
    [HttpGet("matches")]
    public async Task<ActionResult<List<MatchDto>>> GetMatches([FromQuery] MatchStatus? status)
    {
        var query = context.Matches
            .Include(m => m.HomeTeam)
            .Include(m => m.AwayTeam)
            .Include(m => m.Group)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(m => m.Status == status.Value);

        var matches = await query.OrderBy(m => m.MatchDate).ToListAsync();
        return Ok(matches.Select(MatchService.ToMatchDto));
    }

    [HttpPatch("matches/{id:int}/result")]
    public async Task<IActionResult> UpdateResult(int id, [FromBody] UpdateMatchResultRequest request)
    {
        if (request.HomeScore < 0 || request.AwayScore < 0)
            return BadRequest(new { error = "Placar não pode ser negativo." });

        var match = await context.Matches.FindAsync(id)
            ?? throw new KeyNotFoundException("Jogo não encontrado.");

        match.HomeScore = request.HomeScore;
        match.AwayScore = request.AwayScore;
        match.Status = MatchStatus.Finished;
        await context.SaveChangesAsync();

        // Processa pontuação em background
        BackgroundJob.Enqueue<PredictionService>(s => s.ProcessMatchPredictionsAsync(id));

        var home = (await context.Teams.FindAsync(match.HomeTeamId))?.Name ?? "?";
        var away = (await context.Teams.FindAsync(match.AwayTeamId))?.Name ?? "?";

        BackgroundJob.Enqueue<PushNotificationService>(s => s.SendToAllAsync(
            "Resultado disponível!",
            $"{home} {request.HomeScore} x {request.AwayScore} {away}",
            null));

        return NoContent();
    }

    [HttpPost("matches/{id:int}/start")]
    public async Task<IActionResult> StartMatch(int id)
    {
        var match = await context.Matches.FindAsync(id)
            ?? throw new KeyNotFoundException("Jogo não encontrado.");

        match.Status = MatchStatus.InProgress;
        await context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("generate-next-phase")]
    public async Task<IActionResult> GenerateNextPhase()
    {
        await nextPhaseJob.GenerateRoundOf32Async();
        return Ok(new { message = "Oitavas de final geradas com sucesso." });
    }

    [HttpPost("send-notification")]
    public async Task<IActionResult> SendNotification([FromBody] SendNotificationRequest request)
    {
        await pushService.SendToAllAsync(request.Title, request.Body);
        return Ok();
    }

    [HttpPost("users/{userId}/toggle-admin")]
    public async Task<IActionResult> ToggleAdmin(Guid userId)
    {
        var user = await context.Users.FindAsync(userId)
            ?? throw new KeyNotFoundException("Usuário não encontrado.");

        user.IsAdmin = !user.IsAdmin;
        user.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();

        return Ok(new { userId, isAdmin = user.IsAdmin });
    }
}

public record SendNotificationRequest(string Title, string Body);
