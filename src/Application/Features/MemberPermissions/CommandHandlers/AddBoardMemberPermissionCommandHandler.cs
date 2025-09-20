using Application.Abstractions.CQRS;
using Application.Contracts;
using Application.Contracts.Board;
using Application.Features.MemberPermissions.Commands;
using Domain.Constants;
using Domain.MemberPermissions.Entities;
using FluentResults;
using Microsoft.AspNetCore.Http;

namespace Application.Features.MemberPermissions.CommandHandlers;

internal sealed class AddBoardMemberPermissionCommandHandler : ICommandHandler<AddBoardMemberPermissionCommand, MemberPermission>
{
    private readonly IBoardRepository _boardMemberRepository;
    private readonly IUnitOfWork _unitOfWork;
    
    public AddBoardMemberPermissionCommandHandler(IUnitOfWork unitOfWork, IBoardRepository boardMemberRepository)
    {
        _unitOfWork = unitOfWork;
        _boardMemberRepository = boardMemberRepository;
    }
    
    public async Task<Result<MemberPermission>> Handle(AddBoardMemberPermissionCommand command, CancellationToken ct = default)
    {
         var board = await _boardMemberRepository.GetWithMembers(command.BoardId, ct);
         if (board is null)
             return Result.Fail(new Error("Board not found."));
         
         var currentUserMember = board.GetMemberByUserId(command.RequestingUserId);
         if (currentUserMember is null)
             return Result.Fail(new Error("You are not a member of this board."));
         
         if(currentUserMember.RoleId != Roles.OwnerId)
             return Result.Fail(new Error("Only the board owner can add permissions to members.")
                 .WithMetadata("Status", StatusCodes.Status400BadRequest));
         
         var member = board.GetMemberByUserId(command.MemberId);
         if (member is null)
             return Result.Fail(new Error("The specified member does not belong to the board."));
         
         var permissionResult = member.AddPermission(command.Permission, command.RequestingUserId);
         if (permissionResult.IsFailed)
             return Result.Fail(permissionResult.Errors);
            
            
         await _unitOfWork.SaveChangesAsync(ct);
        
         return Result.Ok();
    }
}