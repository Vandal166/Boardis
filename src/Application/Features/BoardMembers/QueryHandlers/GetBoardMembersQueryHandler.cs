using Application.Abstractions.CQRS;
using Application.Contracts.Keycloak;
using Application.DTOs.BoardMembers;
using Application.Features.BoardMembers.Queries;
using Domain.Contracts;
using FluentResults;

namespace Application.Features.BoardMembers.QueryHandlers;

internal sealed class GetBoardMembersQueryHandler : IQueryHandler<GetBoardMembersQuery, List<BoardMemberResponse>>
{
    private readonly IBoardRepository _boardRepository;
    private readonly IBoardMemberRepository _boardMemberRepository;
    private readonly IKeycloakUserService _keycloakUserService;

    public GetBoardMembersQueryHandler(IBoardRepository boardRepository, IBoardMemberRepository boardMemberRepository, IKeycloakUserService keycloakUserService)
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
        
        var members = await _boardMemberRepository.GetByBoardIdAsync(query.BoardId, ct);
        if (members is null)
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
                    Email = userResult.Value.Email
                });
            }
        }
        
        return Result.Ok(memberResponses);
    }
}