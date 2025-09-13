using Application.Abstractions.CQRS;
using Application.Contracts;
using Application.Features.BoardMembers.Commands;
using Domain.Contracts;
using FluentResults;

namespace Application.Features.BoardMembers.CommandHandlers;

internal sealed class RemoveBoardMemberCommandHandler : ICommandHandler<RemoveBoardMemberCommand>
{
    private readonly IBoardRepository _boardRepository;
    private readonly IBoardMemberRepository _boardMemberRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveBoardMemberCommandHandler(IBoardRepository boardRepository, IBoardMemberRepository boardMemberRepository,
        IUnitOfWork unitOfWork)
    {
        _boardRepository = boardRepository;
        _boardMemberRepository = boardMemberRepository;
        _unitOfWork = unitOfWork;
    }
    
    
    public async Task<Result> Handle(RemoveBoardMemberCommand command, CancellationToken ct = default)
    {
        var board = await _boardRepository.GetByIdAsync(command.BoardId, ct);
        if (board is null)
            return Result.Fail("Board not found");
        
        var memberToRemove = await _boardMemberRepository.GetByIdAsync(board.Id, command.UserIdToRemove, ct);
        if (memberToRemove is null)
            return Result.Fail("User to remove is not a member of this board");
        
        if(memberToRemove.UserId == command.RequestingUserId)
            return Result.Fail("You cannot remove yourself from the board");
        
        await _boardMemberRepository.DeleteAsync(memberToRemove, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        
        return Result.Ok();
    }
}