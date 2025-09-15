using Application.Abstractions.CQRS;
using Application.Contracts;
using Application.Contracts.Board;
using Application.Features.MemberPermissions.Commands;
using FluentResults;
using Microsoft.AspNetCore.Http;

namespace Application.Features.MemberPermissions.CommandHandlers;

internal sealed class RemoveBoardMemberPermissionCommandHandler : ICommandHandler<RemoveBoardMemberPermissionCommand>
{
    private readonly IBoardMemberRepository _boardMemberRepository;
    private readonly IUnitOfWork _unitOfWork;
    public RemoveBoardMemberPermissionCommandHandler(IBoardMemberRepository boardMemberRepository, IUnitOfWork unitOfWork)
    {
        _boardMemberRepository = boardMemberRepository;
        _unitOfWork = unitOfWork;
    }
    
    public async Task<Result> Handle(RemoveBoardMemberPermissionCommand command, CancellationToken ct = default)
    {
        var member = await _boardMemberRepository.GetByIdAsync(command.BoardId, command.MemberId, ct);
        if (member is null)
            return Result.Fail(new Error("The specified member does not belong to the board."));
        
        if(command.RequestingUserId == command.MemberId)
            return Result.Fail(new Error("You cannot remove permissions from yourself.")
                .WithMetadata("Status", StatusCodes.Status400BadRequest));
 
        var permissionResult = member.RemovePermission(command.Permission);
        if (permissionResult.IsFailed)
            return Result.Fail(permissionResult.Errors);
        
        
        await _boardMemberRepository.UpdateAsync(member, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Ok();
    }
}