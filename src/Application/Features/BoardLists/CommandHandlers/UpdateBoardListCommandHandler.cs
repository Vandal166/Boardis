using Application.Abstractions.CQRS;
using Application.Contracts;
using Application.Features.BoardLists.Commands;
using Domain.Contracts;
using Domain.ValueObjects;
using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;

namespace Application.Features.BoardLists.CommandHandlers;

internal sealed class UpdateBoardListCommandHandler : ICommandHandler<UpdateBoardListCommand>
{
    private readonly IBoardRepository _boardRepository;
    private readonly IBoardListRepository _boardListRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDistributedCache _cache;
    public UpdateBoardListCommandHandler(IBoardRepository boardRepository, IBoardListRepository boardListRepository, IUnitOfWork unitOfWork, IDistributedCache cache)
    {
        _boardRepository = boardRepository;
        _boardListRepository = boardListRepository;
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task<Result> Handle(UpdateBoardListCommand command, CancellationToken ct = default)
    {
        var board = await _boardRepository.GetByIdAsync(command.BoardId, ct);
        if (board is null)
            return Result.Fail("Board not found");

        //permission check
        var boardMember = board.HasMember(command.RequestingUserId);
        if (boardMember is null)
            return Result.Fail(new Error("You are not a member of this board")
                .WithMetadata("Status", StatusCodes.Status403Forbidden));

        if (!board.MemberHasRole(boardMember.UserId, Role.Owner))
            return Result.Fail(new Error("You don't have permission to update a list in this board")
                .WithMetadata("Status", StatusCodes.Status403Forbidden));

        var boardList = await _boardListRepository.GetByIdAsync(command.BoardListId, ct);
        if (boardList is null || boardList.BoardId != command.BoardId)
            return Result.Fail(new Error("List not found")
                .WithMetadata("Status", StatusCodes.Status404NotFound));

        var errors = boardList.Update(command.Title, command.Position, command.ColorArgb);
        if (errors.IsFailed)
            return errors;

        await _boardListRepository.UpdateAsync(boardList, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        
        // Invalidate cache
        string cacheKey = $"lists_{command.BoardId}";
        await _cache.RemoveAsync(cacheKey, ct);

        return Result.Ok();
    }
}