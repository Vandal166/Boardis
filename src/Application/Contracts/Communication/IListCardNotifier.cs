namespace Application.Contracts.Communication;

public interface IListCardNotifier
{
    Task NotifyListCardCreatedAsync(Guid boardId, Guid boardListId, Guid listCardId, CancellationToken ct = default);
    
    Task NotifyListCardUpdatedAsync(Guid boardId, Guid boardListId, Guid listCardId, CancellationToken ct = default);
    
    Task NotifyListCardDeletedAsync(Guid boardId, Guid boardListId, Guid listCardId, CancellationToken ct = default);
}