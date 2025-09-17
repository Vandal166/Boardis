using Application.Abstractions.CQRS;
using Application.Contracts.User;
using Application.DTOs.MemberPermissions;
using Application.Features.MemberPermissions.Commands;
using Application.Features.MemberPermissions.Queries;
using Domain.Constants;
using Domain.MemberPermissions.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.API.Common;

namespace Web.API.Controllers;

[ApiController]
[Route("api/boards/{boardId:guid}/members/{memberId:guid}/permissions")]
[Authorize]
public sealed class BoardMemberPermissionsController : ControllerBase
{
    private readonly ICommandHandler<AddBoardMemberPermissionCommand, MemberPermission> _addPermissionHandler;
    private readonly ICommandHandler<RemoveBoardMemberPermissionCommand> _removePermissionHandler;
    private readonly IQueryHandler<GetBoardMemberPermissionsQuery, BoardMemberPermissionsResponse> _getPermissionsHandler;
    private readonly ICurrentUser _currentUser;
    
    public BoardMemberPermissionsController(ICommandHandler<AddBoardMemberPermissionCommand, MemberPermission> addPermissionHandler,
        ICommandHandler<RemoveBoardMemberPermissionCommand> removePermissionHandler,
        IQueryHandler<GetBoardMemberPermissionsQuery, BoardMemberPermissionsResponse> getPermissionsHandler, ICurrentUser currentUser)
    {
        _addPermissionHandler = addPermissionHandler;
        _removePermissionHandler = removePermissionHandler;
        _getPermissionsHandler = getPermissionsHandler;
        _currentUser = currentUser;
    }
    
    
    [HttpGet]
    [HasPermission(Permissions.Read)]
    public async Task<IActionResult> GetMemberPermissions(Guid boardId, Guid memberId, CancellationToken ct = default)
    {
        var query = new GetBoardMemberPermissionsQuery
        {
            BoardId = boardId,
            MemberId = memberId,
            RequestingUserId = _currentUser.Id
        };
        var result = await _getPermissionsHandler.Handle(query, ct);
        if (result.IsFailed)
            return result.ToProblemResponse(this, StatusCodes.Status404NotFound);
        
        return Ok(result.Value);
    }
    
    [HttpGet]
    [Route("/api/permissions")]
    public Task<IActionResult> GetAllPermissions(CancellationToken ct = default)
    {
        var permissions = Enum.GetNames<Permissions>();
        return Task.FromResult<IActionResult>(Ok(permissions));
    }

    [HttpPut]
    [HasPermission(Permissions.Update)]
    public async Task<IActionResult> AddPermission(Guid boardId, Guid memberId, [FromBody] AddBoardMemberPermissionRequest request, CancellationToken ct = default)
    {
        if (!Enum.TryParse<Permissions>(request.Permission, out var parsedPermission))
            return Problem(detail: "Invalid permission", statusCode: StatusCodes.Status400BadRequest);
        
        var command = new AddBoardMemberPermissionCommand
        {
            BoardId = boardId,
            MemberId = memberId,
            Permission = parsedPermission,
            RequestingUserId = _currentUser.Id
        };
        
        var result = await _addPermissionHandler.Handle(command, ct);
        if (result.IsFailed)
            return result.ToProblemResponse(this);

        return NoContent();
    }

    [HttpDelete]
    [HasPermission(Permissions.Update)]
    public async Task<IActionResult> RemovePermission(Guid boardId, Guid memberId, [FromBody] RemoveBoardMemberPermissionRequest request, CancellationToken ct = default)
    {
        if (!Enum.TryParse<Permissions>(request.Permission, out var parsedPermission))
            return Problem(detail: "Invalid permission", statusCode: StatusCodes.Status400BadRequest);
        
        var command = new RemoveBoardMemberPermissionCommand
        {
            BoardId = boardId,
            MemberId = memberId,
            Permission = parsedPermission,
            RequestingUserId = _currentUser.Id
        };

        var result = await _removePermissionHandler.Handle(command, ct);
        if (result.IsFailed)
            return result.ToProblemResponse(this);

        return NoContent();
    }
}