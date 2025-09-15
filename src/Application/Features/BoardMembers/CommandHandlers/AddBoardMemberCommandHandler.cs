using Application.Abstractions.CQRS;
using Application.Contracts;
using Application.Contracts.Board;
using Application.Features.BoardMembers.Commands;
using Domain.Constants;
using Domain.Entities;
using FluentResults;
using Microsoft.AspNetCore.Http;

namespace Application.Features.BoardMembers.CommandHandlers;

internal sealed class AddBoardMemberCommandHandler : ICommandHandler<AddBoardMemberCommand, BoardMember>
{
    private readonly IBoardMemberRepository _boardMemberRepository;
    private readonly IBoardRepository _boardRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddBoardMemberCommandHandler(IBoardMemberRepository boardMemberRepository, IBoardRepository boardRepository,
        IUnitOfWork unitOfWork)
    {
        _boardMemberRepository = boardMemberRepository;
        _boardRepository = boardRepository;
        _unitOfWork = unitOfWork;
    }
    
    public async Task<Result<BoardMember>> Handle(AddBoardMemberCommand command, CancellationToken ct = default)
    {
        var board = await _boardRepository.GetByIdAsync(command.BoardId, ct);
        if (board is null)
            return Result.Fail<BoardMember>("Board not found");
        
        var memberToAdd = await _boardMemberRepository.GetByIdAsync(board.Id, command.UserIdToAdd, ct);
        if (memberToAdd is not null)
            return Result.Fail<BoardMember>(new Error("User is already a member of this board")
                .WithMetadata("Status", StatusCodes.Status409Conflict));
        
        var memberResult = BoardMember.Create(board.Id, command.UserIdToAdd, Roles.MemberId);
        if (memberResult.IsFailed)
            return Result.Fail(memberResult.Errors);
        
        var newMember = memberResult.Value;
        newMember.AddPermission(Permissions.Read);
        
        await _boardMemberRepository.AddAsync(newMember, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        
        return Result.Ok(newMember);
    }
}