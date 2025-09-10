using Application.Abstractions.CQRS;
using Application.Contracts;
using Application.Features.BoardLists.Commands;
using Domain.Constants;
using Domain.Contracts;
using Domain.ValueObjects;
using FluentResults;
using Microsoft.Extensions.Caching.Distributed;

namespace Application.Features.BoardLists.CommandHandlers;

internal sealed class DeleteBoardListCommandHandler : ICommandHandler<DeleteBoardListCommand>
{
    private readonly IBoardRepository _boardRepository;
    private readonly IBoardListRepository _boardListRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDistributedCache _cache;
    public DeleteBoardListCommandHandler(IBoardRepository boardRepository, IBoardListRepository boardListRepository,
        IUnitOfWork unitOfWork, IDistributedCache cache)
    {
        _boardRepository = boardRepository;
        _boardListRepository = boardListRepository;
        _unitOfWork = unitOfWork;
        _cache = cache;
    }
    
    public async Task<Result> Handle(DeleteBoardListCommand command, CancellationToken ct = default)
    {
        var board = await _boardRepository.GetByIdAsync(command.BoardId, ct);
        if (board is null)
            return Result.Fail("Board not found");
        
        //permission check
        var boardMember = board.HasMember(command.RequestingUserId);
        if (boardMember is null)
            return Result.Fail("You are not a member of this board");

        if(!board.MemberHasRole(boardMember.UserId, Role.Owner))
            return Result.Fail("You don't have permission to delete a list in this board");
        
        var boardList = await _boardListRepository.GetByIdAsync(command.BoardListId, ct);
        if (boardList is null || boardList.BoardId != command.BoardId) // if the list does not belong to the board
            return Result.Fail("List not found in this board");
        
        await _boardListRepository.DeleteAsync(boardList, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        
        // Invalidate cache
        string cacheKey = $"lists_{command.BoardId}";
        await _cache.RemoveAsync(cacheKey, ct);
        
        return Result.Ok();
    }
}