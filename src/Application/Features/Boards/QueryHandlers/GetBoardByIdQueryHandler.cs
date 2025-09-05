using Application.Abstractions.CQRS;
using Application.DTOs.Boards;
using Application.Features.Boards.Queries;
using Domain.Constants;
using Domain.Contracts;
using FluentResults;
using Microsoft.AspNetCore.Http;

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

        // if board is private, check if user is a member
        if (board.HasVisibility(VisibilityLevel.Private))
        {
            if(board.HasMember(query.RequestingUserId) is null)
                return Result.Fail(new Error("You are not a member of this board")
                    .WithMetadata("Status", StatusCodes.Status403Forbidden));
        }
        
        var boardResponse = new BoardResponse
        {
            Id = board.Id,
            Title = board.Title,
            Description = board.Description,
            WallpaperImageId = board.WallpaperImageId
        };
        
        return Result.Ok(boardResponse);
    }
}