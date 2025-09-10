using Application.Abstractions.CQRS;
using Application.Contracts;
using Application.Features.Boards.Commands;
using Domain.Contracts;
using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;

namespace Application.Features.Boards.CommandHandlers;

internal sealed class PatchBoardCommandHandler : ICommandHandler<PatchBoardCommand>
{
    private readonly IBoardRepository _boardRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDistributedCache _cache;
    public PatchBoardCommandHandler(IBoardRepository boardRepository, IUnitOfWork unitOfWork, IDistributedCache cache)
    {
        _boardRepository = boardRepository;
        _unitOfWork = unitOfWork;
        _cache = cache;
    }
    
    
    public async Task<Result> Handle(PatchBoardCommand command, CancellationToken ct = default)
    {
        var board = await _boardRepository.GetByIdAsync(command.BoardId, ct);
        if (board is null)
            return Result.Fail("Board not found");
        
        //permission check
        var boardMember = board.HasMember(command.RequestingUserId);
        if (boardMember is null)
            return Result.Fail(new Error("You are not a member of this board")
                .WithMetadata("Status", StatusCodes.Status403Forbidden));
        
        if(!board.MemberHasRole(boardMember.UserId, Domain.ValueObjects.Role.Owner))
            return Result.Fail(new Error("You don't have permission to update this board")
                .WithMetadata("Status", StatusCodes.Status403Forbidden));
        
        var updateResult = board.Patch(command.Title, command.Description, command.WallpaperImageId, command.Visibility);
        if (updateResult.IsFailed)
            return Result.Fail(updateResult.Errors);
        
        await _boardRepository.UpdateAsync(board, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        
        // Invalidate cache
        string cacheKey = $"boards_{command.RequestingUserId}";
        await _cache.RemoveAsync(cacheKey, ct);
        
        return Result.Ok();
    }
}