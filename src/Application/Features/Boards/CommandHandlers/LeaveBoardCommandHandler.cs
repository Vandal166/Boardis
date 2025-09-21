using Application.Abstractions.CQRS;
using Application.Contracts;
using Application.Contracts.Board;
using Application.Features.Boards.Commands;
using FluentResults;
using Microsoft.AspNetCore.Http;

namespace Application.Features.Boards.CommandHandlers;

internal sealed class LeaveBoardCommandHandler : ICommandHandler<LeaveBoardCommand>
{
    private readonly IBoardRepository _boardRepository;
    private readonly IUnitOfWork _unitOfWork;
    public LeaveBoardCommandHandler(IBoardRepository boardRepository, IUnitOfWork unitOfWork)
    {
        _boardRepository = boardRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(LeaveBoardCommand command, CancellationToken ct = default)
    {
        var board = await _boardRepository.GetWithMembers(command.BoardId, ct);
        if (board is null)
            return Result.Fail(new Error("BoardNotFound").WithMetadata("Status", StatusCodes.Status404NotFound));
        
        var leaveResult = board.LeaveBoard(command.RequestingUserId);
        if (leaveResult.IsFailed)
            return Result.Fail(leaveResult.Errors);
        
        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Ok();
    }
}