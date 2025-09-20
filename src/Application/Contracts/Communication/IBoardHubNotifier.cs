namespace Application.Contracts.Communication;

public interface IBoardHubNotifier
{
    Task NotifyBoardUpdatedAsync(Guid boardId, CancellationToken ct = default);
    
    Task NotifyBoardDeletedAsync(Guid boardId, CancellationToken ct = default);
}