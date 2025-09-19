using Application.Contracts.Communication;
using Microsoft.AspNetCore.SignalR;
using Web.API.Communication.Hubs;

namespace Web.API.Communication.Services;

internal sealed class ListCardNotifier : IListCardNotifier
{
    private readonly IHubContext<BoardNotificationHub> _hubContext;

    public ListCardNotifier(IHubContext<BoardNotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }   

    public async Task NotifyListCardCreatedAsync(Guid boardId, Guid boardListId, Guid listCardId, CancellationToken ct = default)
    {
        await _hubContext.Clients.Group(boardId.ToString())
            .SendAsync("ListCardCreated", boardId, boardListId, listCardId, ct);
    }

    public async Task NotifyListCardUpdatedAsync(Guid boardId, Guid boardListId, Guid listCardId, CancellationToken ct = default)
    {
        await _hubContext.Clients.Group(boardId.ToString())
            .SendAsync("ListCardUpdated", boardId, boardListId, listCardId, ct);
    }

    public async Task NotifyListCardDeletedAsync(Guid boardId, Guid boardListId, Guid listCardId, CancellationToken ct = default)
    {
        await _hubContext.Clients.Group(boardId.ToString())
            .SendAsync("ListCardDeleted", boardId, boardListId, listCardId, ct);
    }
}