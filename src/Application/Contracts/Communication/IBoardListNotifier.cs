namespace Application.Contracts.Communication;

public interface IBoardListNotifier
{
    Task NotifyBoardListCreatedAsync(Guid boardId, Guid boardListId, CancellationToken ct = default);
    
    Task NotifyBoardListUpdatedAsync(Guid boardId, Guid boardListId, CancellationToken ct = default);
    
    Task NotifyBoardListDeletedAsync(Guid boardId, Guid boardListId, CancellationToken ct = default);
}