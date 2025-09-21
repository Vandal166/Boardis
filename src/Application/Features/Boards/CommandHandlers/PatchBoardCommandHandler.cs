using Application.Abstractions.CQRS;
using Application.Contracts;
using Application.Contracts.Board;
using Application.Features.Boards.Commands;
using FluentResults;

namespace Application.Features.Boards.CommandHandlers;

internal sealed class PatchBoardCommandHandler : ICommandHandler<PatchBoardCommand>
{
    private readonly IBoardRepository _boardRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PatchBoardCommandHandler(IBoardRepository boardRepository,IUnitOfWork unitOfWork)
    {
        _boardRepository = boardRepository;
        _unitOfWork = unitOfWork;
    }
    
    
    public async Task<Result> Handle(PatchBoardCommand command, CancellationToken ct = default)
    {
        var board = await _boardRepository.GetWithMembers(command.BoardId, ct);
        if (board is null)
            return Result.Fail("BoardNotFound");
        
        var updateResult = board.Patch(command.Title, command.Description, command.Visibility, command.RequestingUserId);
        if (updateResult.IsFailed)
            return Result.Fail(updateResult.Errors);

        await _unitOfWork.SaveChangesAsync(ct);
        
        return Result.Ok();
    }
}