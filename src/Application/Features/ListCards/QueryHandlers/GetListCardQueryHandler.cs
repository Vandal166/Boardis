using System.Text.Json;
using Application.Abstractions.CQRS;
using Application.DTOs.ListCards;
using Application.Features.ListCards.Queries;
using Domain.Constants;
using Domain.Contracts;
using FluentResults;
using Microsoft.Extensions.Caching.Distributed;
// ReSharper disable SuggestVarOrType_BuiltInTypes

namespace Application.Features.ListCards.QueryHandlers;

internal sealed class GetListCardQueryHandler : IQueryHandler<GetListCardsQuery, List<ListCardResponse>>
{
    private readonly IBoardRepository _boardRepository;
    private readonly IBoardListRepository _boardListRepository;
    private readonly IListCardRepository _listCardRepository;
    private readonly IDistributedCache _cache;
    
    public GetListCardQueryHandler(IBoardRepository boardRepository, IBoardListRepository boardListRepository, IListCardRepository listCardRepository, IDistributedCache cache)
    {
        _boardRepository = boardRepository;
        _boardListRepository = boardListRepository;
        _listCardRepository = listCardRepository;
        _cache = cache;
    }
    
    public async Task<Result<List<ListCardResponse>>> Handle(GetListCardsQuery query, CancellationToken ct = default)
    {
        var board = await _boardRepository.GetByIdAsync(query.BoardId, ct);
        if (board is null)
            return Result.Fail<List<ListCardResponse>>("Board not found");
        
        if (board.HasVisibility(VisibilityLevel.Private))
        {
            if(board.HasMember(query.RequestingUserId) is null)
                return Result.Fail("You are not a member of this board");
        }
        
        var boardList = await _boardListRepository.GetByBoardIdAsync(query.BoardId, ct);
        if (boardList is null)
            return Result.Fail("Board list not found");
        
        string cacheKey = $"cards_{query.BoardId}_{query.BoardListId}";
        string? cachedJson = await _cache.GetStringAsync(cacheKey, ct);
        if (!string.IsNullOrEmpty(cachedJson))
        {
            var cachedResponses = JsonSerializer.Deserialize<List<ListCardResponse>>(cachedJson);
            Console.WriteLine($"Deserialized {cachedResponses?.Count} items from cache.");
            return Result.Ok(cachedResponses!);
        }
        
        var listCard = await _listCardRepository.GetByBoardListIdAsync(query.BoardListId, ct);
        if (listCard is null)
            return Result.Ok(new List<ListCardResponse>());
        
        var listCardResponses = listCard.Select(card => new ListCardResponse
        {
            Id = card.Id,
            BoardId = query.BoardId,
            BoardListId = card.BoardListId,
            Title = card.Title,
            Description = card.Description,
            CompletedAt = card.CompletedAt,
            Position = card.Position,
        }).ToList();
        
        // Cache the result with 5-minute expiration
        string json = JsonSerializer.Serialize(listCardResponses);
        await _cache.SetStringAsync(cacheKey, json, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
            SlidingExpiration = TimeSpan.FromMinutes(2) // resetting expiration if accessed within 2 minutes
        }, ct);
        
        Console.WriteLine($"Cached {listCardResponses.Count} items.");
        return Result.Ok(listCardResponses);
    }
}