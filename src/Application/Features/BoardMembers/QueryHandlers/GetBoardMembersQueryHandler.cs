using Application.Abstractions.CQRS;
using Application.Contracts.Board;
using Application.Contracts.Keycloak;
using Application.DTOs.BoardMembers;
using Application.Features.BoardMembers.Queries;
using FluentResults;

namespace Application.Features.BoardMembers.QueryHandlers;

internal sealed class GetBoardMembersQueryHandler : IQueryHandler<GetBoardMembersQuery, List<BoardMemberResponse>>
{
    private readonly IBoardRepository _boardRepository;
    private readonly IKeycloakUserService _keycloakUserService;

    public GetBoardMembersQueryHandler(IBoardRepository boardRepository, IKeycloakUserService keycloakUserService)
    {
        _boardRepository = boardRepository;
        _keycloakUserService = keycloakUserService;
    }
    
    public async Task<Result<List<BoardMemberResponse>>> Handle(GetBoardMembersQuery query, CancellationToken ct = default)
    {
        var board = await _boardRepository.GetWithMembers(query.BoardId, ct);
        if (board is null)
            return Result.Fail<List<BoardMemberResponse>>("BoardNotFound");
     
        var memberResponses = new List<BoardMemberResponse>();
        foreach (var member in board.Members)
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