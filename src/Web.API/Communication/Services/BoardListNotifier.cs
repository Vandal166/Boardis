using Application.Contracts.Communication;
using Microsoft.AspNetCore.SignalR;
using Web.API.Communication.Hubs;

namespace Web.API.Communication.Services;

internal sealed class BoardListNotifier : IBoardListNotifier
{
    private readonly IHubContext<BoardNotificationHub> _hubContext;

    public BoardListNotifier(IHubContext<BoardNotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }   

    public async Task NotifyBoardListCreatedAsync(Guid boardId, Guid boardListId, CancellationToken ct = default)
    {
        await _hubContext.Clients.Group(boardId.ToString())
            .SendAsync("BoardListCreated", boardId, boardListId, ct);
    }

    public async Task NotifyBoardListUpdatedAsync(Guid boardId, Guid boardListId, CancellationToken ct = default)
    {
        await _hubContext.Clients.Group(boardId.ToString())
            .SendAsync("BoardListUpdated", boardId, boardListId, ct);
    }

    public async Task NotifyBoardListDeletedAsync(Guid boardId, Guid boardListId, CancellationToken ct = default)
    {
        await _hubContext.Clients.Group(boardId.ToString())
            .SendAsync("BoardListDeleted", boardId, boardListId, ct);
    }
}