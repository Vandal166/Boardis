using Application.Abstractions.CQRS;
using Application.Contracts.Keycloak;
using Application.DTOs.BoardMembers;
using Application.Features.BoardMembers.Queries;
using Domain.Constants;
using Domain.Contracts;
using FluentResults;
using Microsoft.AspNetCore.Http;

namespace Application.Features.BoardMembers.QueryHandlers;

internal sealed class GetBoardMembersQueryHandler : IQueryHandler<GetBoardMembersQuery, List<BoardMemberResponse>>
{
    private readonly IBoardRepository _boardRepository;
    private readonly IBoardMemberRepository _boardMemberRepository;
    private readonly IKeycloakUserService _keycloakUserService;
    
    public GetBoardMembersQueryHandler(IBoardRepository boardRepository, IBoardMemberRepository boardMemberRepository,
        IKeycloakUserService keycloakUserService)
    {
        _boardRepository = boardRepository;
        _boardMemberRepository = boardMemberRepository;
        _keycloakUserService = keycloakUserService;
    }
    
    public async Task<Result<List<BoardMemberResponse>>> Handle(GetBoardMembersQuery query, CancellationToken ct = default)
    {
        var board = await _boardRepository.GetByIdAsync(query.BoardId, ct);
        if (board is null)
            return Result.Fail<List<BoardMemberResponse>>("Board not found");

        // if board is private, check if user is a member
        if (board.HasVisibility(VisibilityLevel.Private))
        {
            if(board.HasMember(query.RequestingUserId) is null)
                return Result.Fail(new Error("You are not a member of this board")
                    .WithMetadata("StatusCode", StatusCodes.Status403Forbidden));
        }
        
        var members = await _boardMemberRepository.GetByBoardIdAsync(query.BoardId, ct);
        if (members is null || members.Count == 0)//TODO if no members ??? how
            return Result.Ok(new List<BoardMemberResponse>());

        var memberResponses = new List<BoardMemberResponse>();
        foreach (var member in members)
        {
            var userResult = await _keycloakUserService.GetUserByIdAsync(member.UserId, ct);
            if (userResult.IsSuccess)
            {
                memberResponses.Add(new BoardMemberResponse
                {
                    UserId = member.UserId,
                    Username = userResult.Value.Username,
                    Email = userResult.Value.Email,
                    Role = member.Role.Key
                });
            }
        }
        
        return Result.Ok(memberResponses);
    }
}