using Application.Abstractions.CQRS;
using Application.Contracts;
using Application.Features.ListCards.Commands;
using Domain.Constants;
using Domain.Contracts;
using Domain.Entities;
using Domain.ValueObjects;
using FluentResults;
using Microsoft.Extensions.Caching.Distributed;

namespace Application.Features.ListCards.CommandHandlers;

internal sealed class DeleteListCardCommandHandler : ICommandHandler<DeleteListCardCommand>
{  
    private readonly IBoardRepository _boardRepository;
    private readonly IBoardListRepository _boardListRepository;
    private readonly IListCardRepository _listCardRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDistributedCache _cache;
    public DeleteListCardCommandHandler(IBoardRepository boardRepository, IBoardListRepository boardListRepository,
        IListCardRepository listCardRepository, IUnitOfWork unitOfWork, IDistributedCache cache)
    {
        _boardRepository = boardRepository;
        _boardListRepository = boardListRepository;
        _listCardRepository = listCardRepository;
        _unitOfWork = unitOfWork;
        _cache = cache;
    }
    
    public async Task<Result> Handle(DeleteListCardCommand command, CancellationToken ct = default)
    {
        var board = await _boardRepository.GetByIdAsync(command.BoardId, ct);
        if (board is null)
            return Result.Fail("Board not found");
        
        //permission check
        var boardMember = board.HasMember(command.RequestingUserId);
        if (boardMember is null)
            return Result.Fail("You are not a member of this board");
        
        if(!board.MemberHasRole(boardMember.UserId, Role.Owner))
            return Result.Fail("You don't have permission to delete a card in this board");
        
        var boardList = await _boardListRepository.GetByIdAsync(command.BoardListId, ct);
        if (boardList is null || boardList.BoardId != command.BoardId) // if the list does not belong to the board
            return Result.Fail("List not found in this board");
        
        var listCard = await _listCardRepository.GetByIdAsync(command.CardId, ct);
        if (listCard is null || listCard.BoardListId != command.BoardListId) // if the card does not belong to the list
            return Result.Fail("Card not found in this list");
        
        await _listCardRepository.DeleteAsync(listCard, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        
        // Invalidate cache
        string cacheKey = $"cards_{command.BoardId}_{command.BoardListId}";
        await _cache.RemoveAsync(cacheKey, ct);
        
        return Result.Ok();
    }
}