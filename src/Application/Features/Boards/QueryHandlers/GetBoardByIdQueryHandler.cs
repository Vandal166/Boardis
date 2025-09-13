using Application.Abstractions.CQRS;
using Application.DTOs.Boards;
using Application.Features.Boards.Queries;
using Domain.Contracts;
using FluentResults;

namespace Application.Features.Boards.QueryHandlers;

internal sealed class GetBoardByIdQueryHandler : IQueryHandler<GetBoardByIdQuery, BoardResponse>
{
    private readonly IBoardRepository _boardRepository;

    public GetBoardByIdQueryHandler(IBoardRepository boardRepository)
    {
        _boardRepository = boardRepository;
    }

    public async Task<Result<BoardResponse>> Handle(GetBoardByIdQuery query, CancellationToken ct = default)
    {
        var board = await _boardRepository.GetByIdAsync(query.BoardId, ct);
        if (board is null)
            return Result.Fail("Board not found");
        
        var boardResponse = new BoardResponse
        {
            Id = board.Id,
            Title = board.Title,
            Description = board.Description,
            WallpaperImageId = board.WallpaperImageId,
            Visibility = board.Visibility,
        };
        
        return Result.Ok(boardResponse);
    }
}