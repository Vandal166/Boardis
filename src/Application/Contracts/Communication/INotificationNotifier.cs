namespace Application.Contracts.Communication;

public interface INotificationNotifier
{
    Task NotifyBoardInviteAsync(Guid boardId, Guid toUserId, string byUser, CancellationToken ct = default);
    Task NotifyBoardMemberRemovedAsync(Guid boardId, Guid removedUserId, string byUser, CancellationToken ct = default);
}