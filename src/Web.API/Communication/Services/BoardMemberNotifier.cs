using Application.Contracts.Communication;
using Microsoft.AspNetCore.SignalR;
using Web.API.Communication.Hubs;

namespace Web.API.Communication.Services;

internal sealed class BoardMemberNotifier : IBoardMemberNotifier
{
    private readonly IHubContext<BoardNotificationHub> _hubContext;

    public BoardMemberNotifier(IHubContext<BoardNotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }   

    public async Task NotifyBoardMemberAddedAsync(Guid boardId, Guid userId, CancellationToken ct = default)
    {
        await _hubContext.Clients.Group(boardId.ToString())
            .SendAsync("BoardMemberAdded", boardId, userId, ct);
    }

    public async Task NotifyBoardMemberRemovedAsync(Guid boardId, Guid userId, CancellationToken ct = default)
    {
        await _hubContext.Clients.Group(boardId.ToString())
            .SendAsync("BoardMemberRemoved", boardId, userId, ct);
    }
    
    public async Task NotifyBoardMemberLeftAsync(Guid boardId, Guid userId, CancellationToken ct = default)
    {
        await _hubContext.Clients.Group(boardId.ToString())
            .SendAsync("BoardMemberLeft", boardId, userId, ct);
    }
}