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

internal sealed class NotificationNotifier : INotificationNotifier
{
    private readonly IHubContext<GeneralNotificationHub> _hubContext;

    public NotificationNotifier(IHubContext<GeneralNotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }   

    public async Task NotifyBoardInviteAsync(Guid boardId, Guid toUserId, string byUser, CancellationToken ct = default)
    {
        var notification = new
        {
            Type = "Invited",
            BoardId = boardId,
            ToUserId = toUserId,
            ByUser = byUser,
            Title = "You have been invited to a board"
        };
        await _hubContext.Clients.User(toUserId.ToString())
            .SendAsync("ReceiveNotification", notification, ct);
    }

    public async Task NotifyBoardMemberRemovedAsync(Guid boardId, Guid removedUserId, string byUser, CancellationToken ct = default)
    {
        var notification = new
        {
            Type = "Removed",
            BoardId = boardId,
            RemovedUserId = removedUserId,
            ByUser = byUser,
            Title = "You have been removed from the board"
        };
        await _hubContext.Clients.User(removedUserId.ToString())
            .SendAsync("ReceiveNotification", notification, ct);
    }
}