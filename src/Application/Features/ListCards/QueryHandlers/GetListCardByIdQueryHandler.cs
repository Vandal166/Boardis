using Application.Abstractions.CQRS;
using Application.DTOs.ListCards;
using Application.Features.ListCards.Queries;
using Domain.Contracts;
using FluentResults;

namespace Application.Features.ListCards.QueryHandlers;

internal sealed class GetListCardByIdQueryHandler : IQueryHandler<GetListCardByIdQuery, ListCardResponse>
{
    private readonly IBoardRepository _boardRepository;
    private readonly IBoardListRepository _boardListRepository;
    private readonly IListCardRepository _listCardRepository;

    public GetListCardByIdQueryHandler(IBoardRepository boardRepository, IBoardListRepository boardListRepository, IListCardRepository listCardRepository)
    {
        _boardRepository = boardRepository;
        _boardListRepository = boardListRepository;
        _listCardRepository = listCardRepository;
    }
    
    public async Task<Result<ListCardResponse>> Handle(GetListCardByIdQuery query, CancellationToken ct = default)
    {
        var board = await _boardRepository.GetByIdAsync(query.BoardId, ct);
        if (board is null)
            return Result.Fail<ListCardResponse>("Board not found");
        
        var boardList = await _boardListRepository.GetByIdAsync(query.BoardListId, ct);
        if (boardList is null || boardList.BoardId != query.BoardId)
            return Result.Fail("Board list not found in this board");
        
        
        var listCard = await _listCardRepository.GetByIdAsync(query.CardId, ct);
        if (listCard is null || listCard.BoardListId != query.BoardListId)
            return Result.Fail("Card not found in this list");
        
        
        return Result.Ok(new ListCardResponse
        {
            Id = listCard.Id,
            BoardId = query.BoardId,
            BoardListId = listCard.BoardListId,
            Title = listCard.Title,
            Description = listCard.Description,
            CompletedAt = listCard.CompletedAt,
            Position = listCard.Position
        });
    }
}