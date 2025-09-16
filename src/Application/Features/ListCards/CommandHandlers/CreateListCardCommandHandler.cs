using Application.Abstractions.CQRS;
using Application.Contracts;
using Application.Contracts.Board;
using Application.Features.ListCards.Commands;
using Domain.Entities;
using Domain.ValueObjects;
using FluentResults;

namespace Application.Features.ListCards.CommandHandlers;

internal sealed class CreateListCardCommandHandler : ICommandHandler<CreateListCardCommand, ListCard>
{
    private readonly IBoardRepository _boardRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateListCardCommandHandler(IBoardRepository boardRepository, IUnitOfWork unitOfWork)
    {
        _boardRepository = boardRepository;
        _unitOfWork = unitOfWork;
    }
    
    
    public async Task<Result<ListCard>> Handle(CreateListCardCommand command, CancellationToken ct = default)
    {
        var board = await _boardRepository.GetWithCards(command.BoardId, ct);
        if (board is null)
            return Result.Fail<ListCard>("Board not found");
        
        var boardList = board.GetListById(command.BoardListId);
        if (boardList is null)
            return Result.Fail<ListCard>("List not found in this board");
        
        var titleResult = Title.TryFrom(command.Title);
        if (!titleResult.IsSuccess)
            return Result.Fail<ListCard>(titleResult.Error.ErrorMessage);
        
        var listCardResult = boardList.AddCard(titleResult.ValueObject, command.Description, command.Position);
        if (listCardResult.IsFailed)
            return Result.Fail<ListCard>(listCardResult.Errors);
        
        await _unitOfWork.SaveChangesAsync(ct);
        
        return Result.Ok(listCardResult.Value);
    }
}