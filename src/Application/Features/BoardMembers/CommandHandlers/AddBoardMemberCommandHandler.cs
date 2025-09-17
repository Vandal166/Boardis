using Application.Abstractions.CQRS;
using Application.Contracts;
using Application.Contracts.Board;
using Application.Features.BoardMembers.Commands;
using Domain.BoardMembers.Entities;
using Domain.Constants;
using FluentResults;

namespace Application.Features.BoardMembers.CommandHandlers;

internal sealed class AddBoardMemberCommandHandler : ICommandHandler<AddBoardMemberCommand, BoardMember>
{
    private readonly IBoardRepository _boardRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddBoardMemberCommandHandler(IBoardRepository boardRepository,
        IUnitOfWork unitOfWork)
    {
        _boardRepository = boardRepository;
        _unitOfWork = unitOfWork;
    }
    
    public async Task<Result<BoardMember>> Handle(AddBoardMemberCommand command, CancellationToken ct = default)
    {
        var board = await _boardRepository.GetWithMembers(command.BoardId, ct);
        if (board is null)
            return Result.Fail<BoardMember>("Board not found");
        
        var memberResult = board.AddMember(command.UserIdToAdd, Roles.MemberId, command.RequestingUserId);
        if (memberResult.IsFailed)
            return Result.Fail(memberResult.Errors);
        
        await _unitOfWork.SaveChangesAsync(ct);
        
        return Result.Ok(memberResult.Value);
    }
}