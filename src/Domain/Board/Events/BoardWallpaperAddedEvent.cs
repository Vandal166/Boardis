using Domain.Common;

namespace Domain.Board.Events;

public sealed record BoardWallpaperAddedEvent(Guid BoardId, Guid? OldWallpaperImageId, Guid WallpaperImageId, Guid AddedByUserId) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}