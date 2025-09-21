using Application.Abstractions.CQRS;
using Application.Contracts;
using Application.Contracts.Board;
using Application.Features.BoardMembers.Commands;
using Domain.Constants;
using FluentResults;

namespace Application.Features.BoardMembers.CommandHandlers;

internal sealed class RemoveBoardMemberCommandHandler : ICommandHandler<RemoveBoardMemberCommand>
{
    private readonly IBoardRepository _boardRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveBoardMemberCommandHandler(IBoardRepository boardRepository, IUnitOfWork unitOfWork)
    {
        _boardRepository = boardRepository;
        _unitOfWork = unitOfWork;
    }
    
    
    public async Task<Result> Handle(RemoveBoardMemberCommand command, CancellationToken ct = default)
    {
        var board = await _boardRepository.GetWithMembers(command.BoardId, ct);
        if (board is null)
            return Result.Fail("BoardNotFound");

        var removeResult = board.RemoveMember(command.UserIdToRemove, command.RequestingUserId);
        if (removeResult.IsFailed)
            return Result.Fail(removeResult.Errors);
        
        await _unitOfWork.SaveChangesAsync(ct);
        
        return Result.Ok();
    }
}