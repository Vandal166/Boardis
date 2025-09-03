using Application.Abstractions.CQRS;
using Application.DTOs.ListCards;
using Application.Features.ListCards.Queries;
using Domain.Constants;
using Domain.Contracts;
using FluentResults;

namespace Application.Features.ListCards.QueryHandlers;

internal sealed class GetListCardQueryHandler : IQueryHandler<GetListCardsQuery, List<ListCardResponse>>
{
    private readonly IBoardRepository _boardRepository;
    private readonly IBoardListRepository _boardListRepository;
    private readonly IListCardRepository _listCardRepository;
    
    public GetListCardQueryHandler(IBoardRepository boardRepository, IBoardListRepository boardListRepository, IListCardRepository listCardRepository)
    {
        _boardRepository = boardRepository;
        _boardListRepository = boardListRepository;
        _listCardRepository = listCardRepository;
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
        
        return Result.Ok(listCardResponses);
    }
}