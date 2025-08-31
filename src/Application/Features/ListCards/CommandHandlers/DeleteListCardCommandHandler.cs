using Application.Abstractions.CQRS;
using Application.Contracts;
using Application.Features.ListCards.Commands;
using Domain.Constants;
using Domain.Contracts;
using Domain.Entities;
using Domain.ValueObjects;
using FluentResults;

namespace Application.Features.ListCards.CommandHandlers;

internal sealed class DeleteListCardCommandHandler : ICommandHandler<DeleteListCardCommand>
{  
    private readonly IBoardRepository _boardRepository;
    private readonly IBoardListRepository _boardListRepository;
    private readonly IListCardRepository _listCardRepository;
    private readonly IUnitOfWork _unitOfWork;
    
    public DeleteListCardCommandHandler(IBoardRepository boardRepository, IBoardListRepository boardListRepository,
        IListCardRepository listCardRepository, IUnitOfWork unitOfWork)
    {
        _boardRepository = boardRepository;
        _boardListRepository = boardListRepository;
        _listCardRepository = listCardRepository;
        _unitOfWork = unitOfWork;
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
        
        if(!board.MemberHasRole(boardMember.UserId, Role.Create("Owner", "Owner").Value))
            return Result.Fail("You don't have permission to delete a card in this board");
        
        var boardList = await _boardListRepository.GetByIdAsync(command.BoardListId, ct);
        if (boardList is null || boardList.BoardId != command.BoardId) // if the list does not belong to the board
            return Result.Fail("List not found in this board");
        
        var listCard = await _listCardRepository.GetByIdAsync(command.CardId, ct);
        if (listCard is null || listCard.BoardListId != command.BoardListId) // if the card does not belong to the list
            return Result.Fail("Card not found in this list");
        
        await _listCardRepository.DeleteAsync(listCard, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        
        return Result.Ok();
    }
}