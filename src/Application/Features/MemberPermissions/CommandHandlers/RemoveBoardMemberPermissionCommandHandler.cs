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
            return Result.Fail(new Error("BoardNotFound"));

        if(command.RequestingUserId == command.MemberId)
            return Result.Fail(new Error("CannotRemoveOwnPermissions")
                .WithMetadata("Status", StatusCodes.Status400BadRequest));

        var currentUserMember = board.GetMemberByUserId(command.RequestingUserId);
        if (currentUserMember is null)
            return Result.Fail(new Error("NotMemberOfBoard"));

        if(currentUserMember.RoleId != Domain.Constants.Roles.OwnerId)
            return Result.Fail(new Error("OnlyOwnerCanRemovePermissions")
                .WithMetadata("Status", StatusCodes.Status400BadRequest));

        var member = board.GetMemberByUserId(command.MemberId);
        if (member is null)
            return Result.Fail(new Error("MemberNotFoundOnBoard"));

        var permissionResult = member.RemovePermission(command.Permission, command.RequestingUserId);
        if (permissionResult.IsFailed)
            return Result.Fail(permissionResult.Errors);

        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Ok();
    }
}