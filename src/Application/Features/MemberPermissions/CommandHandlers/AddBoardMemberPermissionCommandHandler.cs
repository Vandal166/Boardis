using Application.Abstractions.CQRS;
using Application.Contracts;
using Application.Contracts.Board;
using Application.Features.MemberPermissions.Commands;
using Domain.Entities;
using FluentResults;

namespace Application.Features.MemberPermissions.CommandHandlers;

internal sealed class AddBoardMemberPermissionCommandHandler : ICommandHandler<AddBoardMemberPermissionCommand, MemberPermission>
{
    private readonly IBoardMemberRepository _boardMemberRepository;
    private readonly IUnitOfWork _unitOfWork;
    
    public AddBoardMemberPermissionCommandHandler(IBoardMemberRepository boardMemberRepository, IUnitOfWork unitOfWork)
    {
        _boardMemberRepository = boardMemberRepository;
        _unitOfWork = unitOfWork;
    }
    
    public async Task<Result<MemberPermission>> Handle(AddBoardMemberPermissionCommand command, CancellationToken ct = default)
    {
        var member = await _boardMemberRepository.GetByIdAsync(command.BoardId, command.MemberId, ct);
        if (member is null)
            return Result.Fail(new Error("The specified member does not belong to the board."));
        
        
        var permissionResult = member.AddPermission(command.Permission);
        if (permissionResult.IsFailed)
            return Result.Fail(permissionResult.Errors);
        
        
        await _boardMemberRepository.UpdateAsync(member, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        
        return Result.Ok();
    }
}