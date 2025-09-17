using Application.Abstractions.CQRS;
using Application.Contracts;
using Application.Contracts.Board;
using Application.Features.MemberPermissions.Commands;
using FluentResults;
using Microsoft.AspNetCore.Http;

namespace Application.Features.MemberPermissions.CommandHandlers;

internal sealed class RemoveBoardMemberPermissionCommandHandler : ICommandHandler<RemoveBoardMemberPermissionCommand>
{
    private readonly IBoardRepository _boardMemberRepository;
    private readonly IUnitOfWork _unitOfWork;
    public RemoveBoardMemberPermissionCommandHandler(IUnitOfWork unitOfWork, IBoardRepository boardMemberRepository)
    {
        _unitOfWork = unitOfWork;
        _boardMemberRepository = boardMemberRepository;
    }
    
    public async Task<Result> Handle(RemoveBoardMemberPermissionCommand command, CancellationToken ct = default)
    {
        var board = await _boardMemberRepository.GetWithMembers(command.BoardId, ct);
        if (board is null)
            return Result.Fail(new Error("Board not found."));
        
        var member = board.GetMemberByUserId(command.MemberId);
        if (member is null)
            return Result.Fail(new Error("The specified member does not belong to the board."));
        
        if(command.RequestingUserId == command.MemberId)
            return Result.Fail(new Error("You cannot remove permissions from yourself.")
                .WithMetadata("Status", StatusCodes.Status400BadRequest));
        
        var permissionResult = member.RemovePermission(command.Permission, command.RequestingUserId);
        if (permissionResult.IsFailed)
            return Result.Fail(permissionResult.Errors);
        
        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Ok();
    }
}