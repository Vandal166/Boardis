using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Web.API.Communication.Hubs;

[Authorize]
internal sealed class BoardNotificationHub : Hub
{
    public async Task JoinGroup(Guid boardId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, boardId.ToString());
    }
    
    public async Task LeaveGroup(Guid boardId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, boardId.ToString());
    }
}

[Authorize]
internal sealed class GeneralNotificationHub : Hub
{
    
}