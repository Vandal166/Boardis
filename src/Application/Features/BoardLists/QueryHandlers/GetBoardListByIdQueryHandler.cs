using Application.Abstractions.CQRS;
using Application.Contracts.Board;
using Application.DTOs.BoardLists;
using Application.Features.BoardLists.Queries;
using FluentResults;

namespace Application.Features.BoardLists.QueryHandlers;

internal sealed class GetBoardListByIdQueryHandler : IQueryHandler<GetBoardListByIdQuery, BoardListResponse>
{
    private readonly IBoardRepository _boardRepository;

    public GetBoardListByIdQueryHandler(IBoardRepository boardRepository)
    {
        _boardRepository = boardRepository;
    }
    
    public async Task<Result<BoardListResponse>> Handle(GetBoardListByIdQuery query, CancellationToken ct = default)
    {
        var board = await _boardRepository.GetWithLists(query.BoardId, ct);
        if (board is null)
            return Result.Fail<BoardListResponse>("BoardNotFound");
        
        var boardList = board.GetListById(query.BoardListId);
        if (boardList is null)
            return Result.Fail("ListNotFound");
        
        var boardListResponse = new BoardListResponse
        {
            Id = boardList.Id,
            BoardId = boardList.BoardId,
            Title = boardList.Title.Value,
            Position = boardList.Position,
            ColorArgb = boardList.ListColor.ToArgb()
        };
        
        return Result.Ok(boardListResponse);
    }
}