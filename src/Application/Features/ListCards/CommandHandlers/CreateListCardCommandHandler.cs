using Application.Abstractions.CQRS;
using Application.Contracts;
using Application.Features.ListCards.Commands;
using Domain.Contracts;
using Domain.Entities;
using Domain.ValueObjects;
using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;

namespace Application.Features.ListCards.CommandHandlers;

internal sealed class CreateListCardCommandHandler : ICommandHandler<CreateListCardCommand, ListCard>
{
    private readonly IBoardRepository _boardRepository;
    private readonly IBoardListRepository _boardListRepository;
    private readonly IListCardRepository _listCardRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDistributedCache _cache;
    public CreateListCardCommandHandler(IBoardRepository boardRepository, IBoardListRepository boardListRepository,
        IListCardRepository listCardRepository, IUnitOfWork unitOfWork, IDistributedCache cache)
    {
        _boardRepository = boardRepository;
        _boardListRepository = boardListRepository;
        _listCardRepository = listCardRepository;
        _unitOfWork = unitOfWork;
        _cache = cache;
    }
    
    
    public async Task<Result<ListCard>> Handle(CreateListCardCommand command, CancellationToken ct = default)
    {
        var board = await _boardRepository.GetByIdAsync(command.BoardId, ct);
        if (board is null)
            return Result.Fail<ListCard>("Board not found");
        
        //permission check
        var boardMember = board.HasMember(command.RequestingUserId);
        if (boardMember is null)
            return Result.Fail<ListCard>(new Error("You are not a member of this board")
                .WithMetadata("Status", StatusCodes.Status403Forbidden));
        
        if(!board.MemberHasRole(boardMember.UserId, Role.Owner))
            return Result.Fail<ListCard>(new Error("You don't have permission to create a card in this list")
                .WithMetadata("Status", StatusCodes.Status403Forbidden));

        var boardList = await _boardListRepository.GetByIdAsync(command.BoardListId, ct);
        if (boardList is null || boardList.BoardId != command.BoardId)
            return Result.Fail<ListCard>("List not found in this board");
        
        var listCards = await _listCardRepository.GetByBoardListIdAsync(command.BoardListId, ct);
        if (listCards is null)
            return Result.Fail<ListCard>("Error retrieving cards for this list");
        
        // if pos is already taken by another card in the same list
        if (listCards.Any(c => c.Position == command.Position))
            return Result.Fail<ListCard>("Position is already taken by another card in this list");
        
        var listCardResult = ListCard.Create(command.BoardListId, command.Title, command.Position, command.Description);
        if (listCardResult.IsFailed)
            return Result.Fail<ListCard>(listCardResult.Errors);
        
        await _listCardRepository.AddAsync(listCardResult.Value, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        
        // Invalidate cache
        string cacheKey = $"cards_{command.BoardId}_{command.BoardListId}";
        await _cache.RemoveAsync(cacheKey, ct);
        
        return Result.Ok(listCardResult.Value);
    }
}