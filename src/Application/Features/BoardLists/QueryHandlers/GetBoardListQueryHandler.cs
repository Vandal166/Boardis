using Application.Abstractions.CQRS;
using Application.Contracts.Board;
using Application.DTOs.BoardLists;
using Application.Features.BoardLists.Queries;
using FluentResults;

namespace Application.Features.BoardLists.QueryHandlers;

internal sealed class GetBoardListQueryHandler : IQueryHandler<GetBoardListsQuery, List<BoardListResponse>>
{
    private readonly IBoardRepository _boardRepository;

    public GetBoardListQueryHandler(IBoardRepository boardRepository)
    {
        _boardRepository = boardRepository;
    }
    
    public async Task<Result<List<BoardListResponse>>> Handle(GetBoardListsQuery query, CancellationToken ct = default)
    {
        var board = await _boardRepository.GetWithLists(query.BoardId, ct);
        if (board is null)
            return Result.Fail<List<BoardListResponse>>("Board not found");
      
        var boardListResponses = board.Lists.Select(bl => new BoardListResponse
        {
            Id = bl.Id,
            BoardId = bl.BoardId,
            Title = bl.Title.Value,
            Position = bl.Position,
            ColorArgb = bl.ListColor.ToArgb()
        }).ToList();
        
        return Result.Ok(boardListResponses);
    }
}