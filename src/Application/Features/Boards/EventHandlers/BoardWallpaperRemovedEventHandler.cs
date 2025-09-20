using Application.Abstractions.CQRS;
using Application.Contracts;
using Application.Contracts.Media;
using Application.Contracts.Persistence;
using Domain.Board.Events;
using Domain.Constants;
using Microsoft.Extensions.Caching.Distributed;

namespace Application.Features.Boards.EventHandlers;

internal sealed class BoardWallpaperRemovedEventHandler : EventHandlerBase<BoardWallpaperRemovedEvent>
{
    private readonly IBlobStorage _blobStorage;
    private readonly IDistributedCache _cache;
    public BoardWallpaperRemovedEventHandler(IBlobStorage blobStorage, IDistributedCache cache)
    {
        _blobStorage = blobStorage;
        _cache = cache;
    }

    public override async Task Handle(BoardWallpaperRemovedEvent @event, CancellationToken ct = default)
    {
        Console.WriteLine($"BoardWallpaperRemovedEvent handled for BoardId: {@event.BoardId} which occurred on {@event.OccurredOn}");

        if (@event.OldWallpaperImageId is not null)
        {
            await _blobStorage.DeleteFileAsync(@event.OldWallpaperImageId.Value, BlobContainers.MediaContainer, ct);
            await _cache.RemoveAsync($"media_{@event.OldWallpaperImageId}", ct);
        }
    }
}