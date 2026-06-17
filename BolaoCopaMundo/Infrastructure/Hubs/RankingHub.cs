using Microsoft.AspNetCore.SignalR;

namespace BolaoCopaMundo.Infrastructure.Hubs;

public class RankingHub : Hub
{
    public async Task JoinGroupRanking(Guid groupId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"group-ranking-{groupId}");
    }

    public async Task LeaveGroupRanking(Guid groupId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"group-ranking-{groupId}");
    }

    public async Task JoinGlobalRanking()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "global-ranking");
    }

    public async Task LeaveGlobalRanking()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "global-ranking");
    }
}
