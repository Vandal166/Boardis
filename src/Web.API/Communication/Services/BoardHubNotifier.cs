using Application.Contracts.Communication;
using Microsoft.AspNetCore.SignalR;
using Web.API.Communication.Hubs;

namespace Web.API.Communication.Services;

internal sealed class BoardHubNotifier : IBoardHubNotifier
{
    private readonly IHubContext<BoardNotificationHub> _hubContext;

    public BoardHubNotifier(IHubContext<BoardNotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }   

    public async Task NotifyBoardUpdatedAsync(Guid boardId, CancellationToken ct = default)
    {
        await _hubContext.Clients.Group(boardId.ToString())
            .SendAsync("BoardUpdated", boardId, ct);
    }

    public async Task NotifyBoardDeletedAsync(Guid boardId, CancellationToken ct = default)
    {
        await _hubContext.Clients.Group(boardId.ToString())
            .SendAsync("BoardDeleted", boardId, ct);
    }
}