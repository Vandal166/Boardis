using System.Text.Json;
using Application.Abstractions.CQRS;
using Application.Contracts.Keycloak;
using Application.DTOs.BoardMembers;
using Application.Features.BoardMembers.Queries;
using Domain.Constants;
using Domain.Contracts;
using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;

namespace Application.Features.BoardMembers.QueryHandlers;

internal sealed class GetBoardMembersQueryHandler : IQueryHandler<GetBoardMembersQuery, List<BoardMemberResponse>>
{
    private readonly IBoardRepository _boardRepository;
    private readonly IBoardMemberRepository _boardMemberRepository;
    private readonly IKeycloakUserService _keycloakUserService;
    private readonly IDistributedCache _cache;
    public GetBoardMembersQueryHandler(IBoardRepository boardRepository, IBoardMemberRepository boardMemberRepository,
        IKeycloakUserService keycloakUserService, IDistributedCache cache)
    {
        _boardRepository = boardRepository;
        _boardMemberRepository = boardMemberRepository;
        _keycloakUserService = keycloakUserService;
        _cache = cache;
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

        string cacheKey = $"board_members_{query.BoardId}";
        string? cachedJson = await _cache.GetStringAsync(cacheKey, ct);
        if (!string.IsNullOrEmpty(cachedJson))
        {
            var cachedResponses = JsonSerializer.Deserialize<List<BoardMemberResponse>>(cachedJson);
            return Result.Ok(cachedResponses!);
        }
        
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
        
        if(memberResponses.Count > 0)
        {
            var serializedData = JsonSerializer.Serialize(memberResponses);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) // Cache for 10 minutes
            };
            await _cache.SetStringAsync(cacheKey, serializedData, options, ct);
        }
        
        return Result.Ok(memberResponses);
    }
}