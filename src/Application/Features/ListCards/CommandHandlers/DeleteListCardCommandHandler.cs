using Application.Abstractions.CQRS;
using Application.Contracts;
using Application.Contracts.Board;
using Application.Features.ListCards.Commands;
using FluentResults;

namespace Application.Features.ListCards.CommandHandlers;

internal sealed class DeleteListCardCommandHandler : ICommandHandler<DeleteListCardCommand>
{  
    private readonly IBoardRepository _boardRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteListCardCommandHandler(IBoardRepository boardRepository, IUnitOfWork unitOfWork)
    {
        _boardRepository = boardRepository;
        _unitOfWork = unitOfWork;
    }
    
    public async Task<Result> Handle(DeleteListCardCommand command, CancellationToken ct = default)
    {
        var board = await _boardRepository.GetWithCards(command.BoardId, ct);
        if (board is null)
            return Result.Fail("Board not found");
        
        var boardList = board.GetListById(command.BoardListId);
        if (boardList is null)
            return Result.Fail("List not found in this board");
        
        var removeResult = boardList.RemoveCard(command.CardId);
        if (removeResult.IsFailed)
            return Result.Fail(removeResult.Errors);
        
        await _unitOfWork.SaveChangesAsync(ct);
        
        return Result.Ok();
    }
}