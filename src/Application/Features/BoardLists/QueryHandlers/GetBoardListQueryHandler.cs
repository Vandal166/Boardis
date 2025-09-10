using System.Text.Json;
using Application.Abstractions.CQRS;
using Application.DTOs.BoardLists;
using Application.Features.BoardLists.Queries;
using Domain.Constants;
using Domain.Contracts;
using FluentResults;
using Microsoft.Extensions.Caching.Distributed;

namespace Application.Features.BoardLists.QueryHandlers;

internal sealed class GetBoardListQueryHandler : IQueryHandler<GetBoardListsQuery, List<BoardListResponse>>
{
    private readonly IBoardRepository _boardRepository;
    private readonly IBoardListRepository _boardListRepository;
    private readonly IDistributedCache _cache;
    public GetBoardListQueryHandler(IBoardRepository boardRepository, IBoardListRepository boardListRepository, IDistributedCache cache)
    {
        _boardRepository = boardRepository;
        _boardListRepository = boardListRepository;
        _cache = cache;
    }
    
    public async Task<Result<List<BoardListResponse>>> Handle(GetBoardListsQuery query, CancellationToken ct = default)
    {
        var board = await _boardRepository.GetByIdAsync(query.BoardId, ct);
        if (board is null)
            return Result.Fail<List<BoardListResponse>>("Board not found");
        
        if (board.HasVisibility(VisibilityLevel.Private))
        {
            if(board.HasMember(query.RequestingUserId) is null)
                return Result.Fail("You are not a member of this board");
        }
        
        string cacheKey = $"lists_{query.BoardId}";
        string? cachedJson = await _cache.GetStringAsync(cacheKey, ct);
        if (!string.IsNullOrEmpty(cachedJson))
        {
            var cachedResponses = JsonSerializer.Deserialize<List<BoardListResponse>>(cachedJson);
           
            return Result.Ok(cachedResponses!);
        }
        
        var boardList = await _boardListRepository.GetByBoardIdAsync(query.BoardId, ct);
        if (boardList is null)
            return Result.Ok(new List<BoardListResponse>());
        
        var boardListResponses = boardList.Select(bl => new BoardListResponse
        {
            Id = bl.Id,
            BoardId = bl.BoardId,
            Title = bl.Title,
            Position = bl.Position,
            ColorArgb = bl.ListColor.ToArgb()
        }).ToList();

        if (boardListResponses.Count > 0)
        {
            var json = JsonSerializer.Serialize(boardListResponses);
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) // Cache for 10 minutes
            };
            await _cache.SetStringAsync(cacheKey, json, cacheOptions, ct);
        }
        
        return Result.Ok(boardListResponses);
    }
}