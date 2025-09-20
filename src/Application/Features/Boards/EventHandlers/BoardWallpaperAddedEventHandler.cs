using Application.Abstractions.CQRS;
using Application.Contracts;
using Application.Contracts.Media;
using Application.Contracts.Persistence;
using Domain.Board.Events;
using Domain.Constants;
using Microsoft.Extensions.Caching.Distributed;

namespace Application.Features.Boards.EventHandlers;

internal sealed class BoardWallpaperAddedEventHandler : EventHandlerBase<BoardWallpaperAddedEvent>
{
    private readonly IBlobStorage _blobStorage;
    private readonly IDistributedCache _cache;
    public BoardWallpaperAddedEventHandler(IBlobStorage blobStorage, IDistributedCache cache)
    {
        _blobStorage = blobStorage;
        _cache = cache;
    }

    public override async Task Handle(BoardWallpaperAddedEvent @event, CancellationToken ct = default)
    {
        Console.WriteLine($"BoardWallpaperAddedEvent handled for BoardId: {@event.BoardId} which occurred on {@event.OccurredOn}");

        if (@event.OldWallpaperImageId is not null)
        {
            await _blobStorage.DeleteFileAsync(@event.OldWallpaperImageId.Value, BlobContainers.MediaContainer, ct);
            await _cache.RemoveAsync($"media_{@event.OldWallpaperImageId}", ct);
        }
    }
}