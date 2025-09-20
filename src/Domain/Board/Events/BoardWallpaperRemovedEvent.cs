using Domain.Common;

namespace Domain.Board.Events;

public sealed record BoardWallpaperRemovedEvent(Guid BoardId, Guid? OldWallpaperImageId, Guid RemovedByUserId) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}