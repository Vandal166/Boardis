using Application.Abstractions.CQRS;
using Application.DTOs.Boards;
using Application.Features.Boards.Queries;
using Domain.Contracts;
using FluentResults;

namespace Application.Features.Boards.QueryHandlers;

internal sealed class GetBoardsQueryHandler : IQueryHandler<GetBoardsQuery, List<BoardResponse>>
{
    private readonly IBoardRepository _boardRepository;

    public GetBoardsQueryHandler(IBoardRepository boardRepository)
    {
        _boardRepository = boardRepository;
    }

    public async Task<Result<List<BoardResponse>>> Handle(GetBoardsQuery query, CancellationToken ct = default)
    {
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

        return Result.Ok(boardResponses);
    }
}