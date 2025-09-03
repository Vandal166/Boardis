using Application.Abstractions.CQRS;
using Application.Contracts;
using Application.Features.BoardMembers.Commands;
using Domain.Contracts;
using Domain.ValueObjects;
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
        
        var removeResult = board.RemoveMember(command.UserIdToRemove, command.RequestingUserId);
        if (removeResult.IsFailed)
            return Result.Fail(removeResult.Errors);
        
        var member = await _boardMemberRepository.GetByIdAsync(command.BoardId, command.UserIdToRemove, ct);
        if (member is null)
            return Result.Fail("Board member not found");
        
        await _boardMemberRepository.DeleteAsync(member, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        
        return Result.Ok();
    }
}