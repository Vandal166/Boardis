using Application.Abstractions.CQRS;
using Application.DTOs.BoardLists;
using Application.Features.BoardLists.Queries;
using Domain.Contracts;
using FluentResults;

namespace Application.Features.BoardLists.QueryHandlers;

internal sealed class GetBoardListByIdQueryHandler : IQueryHandler<GetBoardListByIdQuery, BoardListResponse>
{
    private readonly IBoardRepository _boardRepository;
    private readonly IBoardListRepository _boardListRepository;

    public GetBoardListByIdQueryHandler(IBoardRepository boardRepository, IBoardListRepository boardListRepository)
    {
        _boardRepository = boardRepository;
        _boardListRepository = boardListRepository;
    }
    
    public async Task<Result<BoardListResponse>> Handle(GetBoardListByIdQuery query, CancellationToken ct = default)
    {
        var board = await _boardRepository.GetByIdAsync(query.BoardId, ct);
        if (board is null)
            return Result.Fail<BoardListResponse>("Board not found");
        
        var boardList = await _boardListRepository.GetByIdAsync(query.BoardListId, ct);
        if (boardList is null)
            return Result.Fail("Board list not found");
        
        var boardListResponse = new BoardListResponse
        {
            Id = boardList.Id,
            BoardId = boardList.BoardId,
            Title = boardList.Title,
            Position = boardList.Position,
            ColorArgb = boardList.ListColor.ToArgb()
        };
        
        return Result.Ok(boardListResponse);
    }
}