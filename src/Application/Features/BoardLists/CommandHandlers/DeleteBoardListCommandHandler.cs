using Application.Abstractions.CQRS;
using Application.Contracts;
using Application.Contracts.Board;
using Application.Features.BoardLists.Commands;
using FluentResults;

namespace Application.Features.BoardLists.CommandHandlers;

internal sealed class DeleteBoardListCommandHandler : ICommandHandler<DeleteBoardListCommand>
{
    private readonly IBoardRepository _boardRepository;
    private readonly IBoardListRepository _boardListRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteBoardListCommandHandler(IBoardRepository boardRepository, IBoardListRepository boardListRepository,
        IUnitOfWork unitOfWork)
    {
        _boardRepository = boardRepository;
        _boardListRepository = boardListRepository;
        _unitOfWork = unitOfWork;
    }
    
    public async Task<Result> Handle(DeleteBoardListCommand command, CancellationToken ct = default)
    {
        var board = await _boardRepository.GetByIdAsync(command.BoardId, ct);
        if (board is null)
            return Result.Fail("Board not found");
        
        var boardList = await _boardListRepository.GetByIdAsync(command.BoardListId, ct);
        if (boardList is null || boardList.BoardId != command.BoardId) // if the list does not belong to the board
            return Result.Fail("List not found in this board");
        
        await _boardListRepository.DeleteAsync(boardList, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        
        return Result.Ok();
    }
}