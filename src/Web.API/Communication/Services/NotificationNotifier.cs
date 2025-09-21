using Application.Contracts.Communication;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Localization;
using Web.API.Communication.Hubs;
using Web.Resources.Resources.Boards;

namespace Web.API.Communication.Services;

internal sealed class NotificationNotifier : INotificationNotifier
{
    private readonly IHubContext<GeneralNotificationHub> _hubContext;
    private readonly IStringLocalizer<BoardResources> _localizer;

    public NotificationNotifier(IHubContext<GeneralNotificationHub> hubContext, IStringLocalizer<BoardResources> localizer)
    {
        _hubContext = hubContext;
        _localizer = localizer;
    }   

    public async Task NotifyBoardInviteAsync(Guid boardId, Guid toUserId, string byUser, CancellationToken ct = default)
    {
        var notification = new
        {
            Type = "Invited",
            BoardId = boardId,
            ToUserId = toUserId,
            ByUser = byUser,
            Title = _localizer["NotificationBoardInviteTitle"].Value
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
            Title = _localizer["NotificationBoardRemovedTitle"].Value
        };
        await _hubContext.Clients.User(removedUserId.ToString())
            .SendAsync("ReceiveNotification", notification, ct);
    }
}