using Application.Contracts.Board;
using Application.Contracts.User;
using Domain.Constants;
using Microsoft.AspNetCore.Authorization;

namespace Web.API.Common;

/// <summary>
/// Determines if a user will be granted access to a resource based on their permissions.
/// </summary>
internal sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly ICurrentUser _currentUser;
    private readonly IBoardMemberRepository _boardMemberRepository;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly IBoardMemberPermissionRepository _memberPermissionRepo;
    public PermissionAuthorizationHandler(ICurrentUser currentUser, IBoardMemberRepository boardMemberRepository,
        IHttpContextAccessor contextAccessor, IBoardMemberPermissionRepository memberPermissionRepo)
    {
        _currentUser = currentUser;
        _boardMemberRepository = boardMemberRepository;
        _contextAccessor = contextAccessor;
        _memberPermissionRepo = memberPermissionRepo;
    }
    
    
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        // this will try to get boardId from route values such as /api/boards/{boardId}
        var boardId = _contextAccessor.HttpContext?.Request.RouteValues["boardId"]?.ToString();
        if (boardId is null || !Guid.TryParse(boardId, out var boardGuid))
        {
            context.Fail(new AuthorizationFailureReason(this, "BoardId is missing or invalid in route"));
            return;
        }
        
        var member = await _boardMemberRepository.GetByIdAsync(boardGuid, _currentUser.Id);
        if (member is null)
        {
            context.Fail(new AuthorizationFailureReason(this, "You are not a member of the board"));
            return;
        }

        // getting the user's permissions
        var rolePermissions = await _memberPermissionRepo.GetByIdAsync(boardGuid, _currentUser.Id);
        if (rolePermissions is null)
        {
            context.Fail(new AuthorizationFailureReason(this, "Could not retrieve user permissions"));
            return;
        }
        
        Permissions requiredPermission = Enum.Parse<Permissions>(requirement.Permission);
        // if the role has the required permission, succeed the requirement
        if (rolePermissions.Any(p => p.Permission == requiredPermission))
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail(new AuthorizationFailureReason(this, $"You lack the required permission: {requirement.Permission}"));
        }
    }
}