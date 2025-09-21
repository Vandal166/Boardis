using Application.Abstractions.CQRS;
using Application.Contracts;
using Application.Contracts.Board;
using Application.Features.BoardLists.Commands;
using FluentResults;

namespace Application.Features.BoardLists.CommandHandlers;

internal sealed class PatchBoardListCommandHandler : ICommandHandler<PatchBoardListCommand>
{
    private readonly IBoardRepository _boardRepository;
    private readonly IUnitOfWork _unitOfWork;
    
    public PatchBoardListCommandHandler(IBoardRepository boardRepository, IUnitOfWork unitOfWork)
    {
        _boardRepository = boardRepository;
        _unitOfWork = unitOfWork;
    }
    
    
    public async Task<Result> Handle(PatchBoardListCommand command, CancellationToken ct = default)
    {
        var board = await _boardRepository.GetWithLists(command.BoardId, ct);
        if (board is null)
            return Result.Fail("BoardNotFound");
        
        var patchResult = board.UpdateList(command.BoardListId, command.Title, command.Position, command.ColorArgb);
        if (patchResult.IsFailed)
            return Result.Fail(patchResult.Errors);
        
        await _unitOfWork.SaveChangesAsync(ct);
        
        return Result.Ok();
    }
}