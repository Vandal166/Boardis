using Application.Abstractions.CQRS;
using Application.Contracts.Board;
using Application.DTOs.MemberPermissions;
using Application.Features.MemberPermissions.Queries;
using FluentResults;
using Microsoft.AspNetCore.Http;

namespace Application.Features.MemberPermissions.QueryHandlers;

internal sealed class GetBoardMemberPermissionsQueryHandler : IQueryHandler<GetBoardMemberPermissionsQuery, BoardMemberPermissionsResponse>
{
    private readonly IBoardRepository _boardRepository;
    public GetBoardMemberPermissionsQueryHandler(IBoardRepository boardRepository)
    {
        _boardRepository = boardRepository;
    }

    public async Task<Result<BoardMemberPermissionsResponse>> Handle(GetBoardMemberPermissionsQuery query, CancellationToken ct = default)
    {
        var board = await _boardRepository.GetWithMembers(query.BoardId, ct);
        if (board is null)
            return Result.Fail(new Error("BoardNotFound")
                .WithMetadata("Status", StatusCodes.Status404NotFound));

        var member = board.Members.FirstOrDefault(m => m.UserId == query.MemberId);
        if (member is null)
            return Result.Fail(new Error("MemberNotFoundOnBoard")
                .WithMetadata("Status", StatusCodes.Status404NotFound));

        var response = new BoardMemberPermissionsResponse
        {
            UserId = query.MemberId,
            Permissions = member.Permissions.Select(p => p.Permission.ToString()).ToList()
        };

        return Result.Ok(response);
    }
}