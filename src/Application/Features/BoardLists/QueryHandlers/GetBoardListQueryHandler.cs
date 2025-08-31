using Application.Abstractions.CQRS;
using Application.DTOs.BoardLists;
using Application.Features.BoardLists.Queries;
using Domain.Constants;
using Domain.Contracts;
using FluentResults;

namespace Application.Features.BoardLists.QueryHandlers;

internal sealed class GetBoardListQueryHandler : IQueryHandler<GetBoardListsQuery, List<BoardListResponse>>
{
    private readonly IBoardRepository _boardRepository;
    private readonly IBoardListRepository _boardListRepository;
    public GetBoardListQueryHandler(IBoardRepository boardRepository, IBoardListRepository boardListRepository)
    {
        _boardRepository = boardRepository;
        _boardListRepository = boardListRepository;
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
        
        return Result.Ok(boardListResponses);
    }
}