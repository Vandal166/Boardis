using Application.Abstractions.CQRS;
using Application.Contracts;
using Application.Contracts.Board;
using Application.Features.BoardLists.Commands;
using FluentResults;

namespace Application.Features.BoardLists.CommandHandlers;

internal sealed class DeleteBoardListCommandHandler : ICommandHandler<DeleteBoardListCommand>
{
    private readonly IBoardRepository _boardRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteBoardListCommandHandler(IBoardRepository boardRepository, IUnitOfWork unitOfWork)
    {
        _boardRepository = boardRepository;
        _unitOfWork = unitOfWork;
    }
    
    public async Task<Result> Handle(DeleteBoardListCommand command, CancellationToken ct = default)
    {
        var board = await _boardRepository.GetWithLists(command.BoardId, ct);
        if (board is null)
            return Result.Fail("BoardNotFound");
        
        var result = board.RemoveList(command.BoardListId);
        if (result.IsFailed)
            return Result.Fail(result.Errors);
        
        await _unitOfWork.SaveChangesAsync(ct);
        
        return Result.Ok();
    }
}