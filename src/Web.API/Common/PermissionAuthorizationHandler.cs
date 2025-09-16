using Application.Contracts.Persistence;
using Application.Contracts.User;
using Dapper;
using Microsoft.AspNetCore.Authorization;

namespace Web.API.Common;

/// <summary>
/// Determines if a user will be granted access to a resource based on their permissions.
/// </summary>
internal sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly ICurrentUser _currentUser;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly IDbConnectionFactory _dbConnectionFactory;
    
    public PermissionAuthorizationHandler(ICurrentUser currentUser,
        IHttpContextAccessor contextAccessor, IDbConnectionFactory dbConnectionFactory)
    {
        _currentUser = currentUser;
        _contextAccessor = contextAccessor;
        _dbConnectionFactory = dbConnectionFactory;
    }
    
    
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var ct = _contextAccessor.HttpContext?.RequestAborted ?? CancellationToken.None;

        // this will try to get boardId from route values such as /api/boards/{boardId}
        var boardId = _contextAccessor.HttpContext?.Request.RouteValues["boardId"]?.ToString();
        if (boardId is null || !Guid.TryParse(boardId, out var boardGuid))
        {
            context.Fail(new AuthorizationFailureReason(this, "BoardId is missing or invalid in route"));
            return;
        }
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(ct);
        
        const string sql = """
                           SELECT
                               EXISTS (SELECT 1 FROM "Boards" b WHERE b."Id" = @BoardId) AS BoardExists,
                               EXISTS (SELECT 1 FROM "BoardMembers" bm WHERE bm."BoardId" = @BoardId AND bm."UserId" = @UserId) AS MemberExists,
                               COALESCE((
                                   SELECT array_agg(mp."Permission")
                                   FROM "MemberPermissions" mp
                                   WHERE mp."BoardId" = @BoardId AND mp."UserId" = @UserId
                               ), ARRAY[]::text[]) AS Permissions;
                           """;

        var result = await connection.QuerySingleAsync<(bool BoardExists, bool MemberExists, string[] Permissions)>(
            new CommandDefinition(sql, new { BoardId = boardGuid, UserId = _currentUser.Id }, cancellationToken: ct));

        if (!result.BoardExists)
        {
            context.Fail(new AuthorizationFailureReason(this, "Board not found"));
            return;
        }

        if (!result.MemberExists)
        {
            context.Fail(new AuthorizationFailureReason(this, "You are not a member of the board"));
            return;
        }
        
        // getting the user's permissions
        var permissionSet = new HashSet<string>(result.Permissions, StringComparer.Ordinal);
        if (permissionSet.Count == 0)
        {
            context.Fail(new AuthorizationFailureReason(this, "Could not retrieve user permissions"));
            return;
        }

        if (!permissionSet.Contains(requirement.Permission))
        {
            context.Fail(new AuthorizationFailureReason(this, $"You lack the required permission: {requirement.Permission}"));
            return;
        }
        
        context.Succeed(requirement);
    }
}