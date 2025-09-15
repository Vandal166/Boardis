using Application.Abstractions.CQRS;
using Application.Contracts.Board;
using Application.DTOs.MemberPermissions;
using Application.Features.MemberPermissions.Queries;
using FluentResults;
using Microsoft.AspNetCore.Http;

namespace Application.Features.MemberPermissions.QueryHandlers;

internal sealed class GetBoardMemberPermissionsQueryHandler : IQueryHandler<GetBoardMemberPermissionsQuery, BoardMemberPermissionsResponse>
{
    private readonly IBoardMemberPermissionRepository _memberPermissionRepo;

    public GetBoardMemberPermissionsQueryHandler(IBoardMemberPermissionRepository memberPermissionRepo)
    {
        _memberPermissionRepo = memberPermissionRepo;
    }

    public async Task<Result<BoardMemberPermissionsResponse>> Handle(GetBoardMemberPermissionsQuery query, CancellationToken ct = default)
    {
        var permissions = await _memberPermissionRepo.GetByIdAsync(query.BoardId, query.MemberId, ct);
        if (permissions is null || permissions.Count == 0)
            return Result.Fail(new Error("No permissions found for the specified member on this board.")
                .WithMetadata("Status", StatusCodes.Status404NotFound));

        var response = new BoardMemberPermissionsResponse
        {
            UserId = query.MemberId,
            Permissions = permissions.Select(p => p.Permission.ToString()).ToList()
        };
        
        return Result.Ok(response);
    }
}