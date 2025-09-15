using System.Text.Json;
using Application.Abstractions.CQRS;
using Application.Contracts.Board;
using Application.DTOs.Boards;
using Application.Features.Boards.Queries;
using FluentResults;
using Microsoft.Extensions.Caching.Distributed;

namespace Application.Features.Boards.QueryHandlers;

internal sealed class GetBoardsQueryHandler : IQueryHandler<GetBoardsQuery, List<BoardResponse>>
{
    private readonly IBoardRepository _boardRepository;
    private readonly IDistributedCache _cache;
    public GetBoardsQueryHandler(IBoardRepository boardRepository, IDistributedCache cache)
    {
        _boardRepository = boardRepository;
        _cache = cache;
    }

    public async Task<Result<List<BoardResponse>>> Handle(GetBoardsQuery query, CancellationToken ct = default)
    {
        string cacheKey = $"boards_{query.RequestingUserId}";
        string? cachedJson = await _cache.GetStringAsync(cacheKey, ct);
        if (!string.IsNullOrEmpty(cachedJson))
        {
            var cachedResponses = JsonSerializer.Deserialize<List<BoardResponse>>(cachedJson);
            return Result.Ok(cachedResponses!);
        }
        
        var boards = await _boardRepository.GetByUserIdAsync(query.RequestingUserId, ct);
        if (boards is null || boards.Count == 0)
            return Result.Ok(new List<BoardResponse>());

        var boardResponses = boards.Select(board => new BoardResponse
        {
            Id = board.Id,
            Title = board.Title,
            Description = board.Description,
            WallpaperImageId = board.WallpaperImageId
        }).ToList();
        
        if(boardResponses.Count > 0)
        {
            var serialized = JsonSerializer.Serialize(boardResponses);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) // Cache for 10 minutes
            };
            await _cache.SetStringAsync(cacheKey, serialized, options, ct);
        }
        

        return Result.Ok(boardResponses);
    }
}