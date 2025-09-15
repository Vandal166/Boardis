using Application.Abstractions.CQRS;
using Application.Contracts;
using Application.Contracts.Board;
using Application.Features.Boards.Commands;
using Domain.Entities;
using FluentResults;
using Microsoft.Extensions.Caching.Distributed;

namespace Application.Features.Boards.CommandHandlers;

internal sealed class PatchBoardCommandHandler : ICommandHandler<PatchBoardCommand>
{
    private readonly IBoardRepository _boardRepository;
    private readonly IBoardMemberRepository _boardMemberRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDistributedCache _cache;
    public PatchBoardCommandHandler(IBoardRepository boardRepository, IBoardMemberRepository boardMemberRepository, IUnitOfWork unitOfWork, IDistributedCache cache)
    {
        _boardRepository = boardRepository;
        _boardMemberRepository = boardMemberRepository;
        _unitOfWork = unitOfWork;
        _cache = cache;
    }
    
    
    public async Task<Result> Handle(PatchBoardCommand command, CancellationToken ct = default)
    {
        var board = await _boardRepository.GetByIdAsync(command.BoardId, ct);
        if (board is null)
            return Result.Fail("Board not found");
        
        var updateResult = board.Patch(command.Title, command.Description, command.WallpaperImageId, command.Visibility);
        if (updateResult.IsFailed)
            return Result.Fail(updateResult.Errors);
        
        await _boardRepository.UpdateAsync(board, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        
        // Invalidate cache
        string cacheKey = $"boards_{command.RequestingUserId}";
        await _cache.RemoveAsync(cacheKey, ct);
        
        // Invalidate board members cache
        var members = await _boardMemberRepository.GetByBoardIdAsync(board.Id, ct);
        foreach (var memberInBoard in members ?? new List<BoardMember>())
        {
            string memberCacheKey = $"boards_{memberInBoard.UserId}";
            await _cache.RemoveAsync(memberCacheKey, ct);
        }
        
        return Result.Ok();
    }
}