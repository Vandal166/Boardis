using Application.Abstractions.CQRS;
using Application.Contracts;
using Application.Features.BoardLists.Commands;
using Domain.Constants;
using Domain.Contracts;
using Domain.Entities;
using Domain.ValueObjects;
using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;

namespace Application.Features.BoardLists.CommandHandlers;

internal sealed class CreateBoardListCommandHandler : ICommandHandler<CreateBoardListCommand, BoardList>
{
    private readonly IBoardRepository _boardRepository;
    private readonly IBoardListRepository _boardListRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDistributedCache _cache;
    public CreateBoardListCommandHandler(IBoardRepository boardRepository, IBoardListRepository boardListRepository, IUnitOfWork unitOfWork, IDistributedCache cache)
    {
        _boardRepository = boardRepository;
        _boardListRepository = boardListRepository;
        _unitOfWork = unitOfWork;
        _cache = cache;
    }
    
    
    public async Task<Result<BoardList>> Handle(CreateBoardListCommand command, CancellationToken ct = default)
    {
        var board = await _boardRepository.GetByIdAsync(command.BoardId, ct);
        if (board is null)
            return Result.Fail<BoardList>("Board not found");
        
        //permission check
        var boardMember = board.HasMember(command.RequestingUserId);
        if (boardMember is null)
            return Result.Fail<BoardList>(new Error("You are not a member of this board")
                .WithMetadata("Status", StatusCodes.Status403Forbidden));
        
        if(!board.MemberHasRole(boardMember.UserId, Role.Owner))
            return Result.Fail<BoardList>(new Error("You don't have permission to create a list in this board")
                .WithMetadata("Status", StatusCodes.Status403Forbidden));
        
        if(board.BoardLists.Any(l => l.Position == command.Position))
            return Result.Fail<BoardList>(new Error("A list in the same position already exists in this board. Reorder the existing lists first.")
                .WithMetadata("Status", StatusCodes.Status409Conflict));
        
        var boardListResult = BoardList.Create(command.BoardId, command.Title, command.Position);
        if (boardListResult.IsFailed)
            return Result.Fail<BoardList>(boardListResult.Errors);
        
        var boardList = boardListResult.Value;
        
        await _boardListRepository.AddAsync(boardList, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        
        // Invalidate cache
        string cacheKey = $"lists_{command.BoardId}";
        await _cache.RemoveAsync(cacheKey, ct);
        
        return Result.Ok(boardList);
    }
}