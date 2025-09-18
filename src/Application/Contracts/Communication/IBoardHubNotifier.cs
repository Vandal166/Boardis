namespace Application.Contracts.Communication;

public interface IBoardHubNotifier
{
    Task NotifyBoardUpdatedAsync(Guid boardId, CancellationToken ct = default);
    
    Task NotifyBoardDeletedAsync(Guid boardId, CancellationToken ct = default);
}

public interface INotificationNotifier
{
    Task NotifyBoardInviteAsync(Guid boardId, Guid toUserId, string byUser, CancellationToken ct = default);
    Task NotifyBoardMemberRemovedAsync(Guid boardId, Guid removedUserId, string byUser, CancellationToken ct = default);
}