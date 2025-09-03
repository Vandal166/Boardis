using Application.Abstractions.CQRS;
using Application.Contracts.User;
using Application.DTOs.BoardMembers;
using Application.Features.BoardMembers.Commands;
using Application.Features.BoardMembers.Queries;
using Domain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.API.Controllers;

[ApiController]
[Route("api/boards/{boardId:guid}/members")]
[Authorize]
public sealed class BoardMembersController : ControllerBase
{
    private readonly ICommandHandler<AddBoardMemberCommand, BoardMember> _addBoardMemberHandler;
    private readonly ICommandHandler<RemoveBoardMemberCommand> _removeBoardMemberHandler;
    private readonly IQueryHandler<GetBoardMembersQuery, List<BoardMemberResponse>> _getBoardMembersHandler;
    private readonly IValidator<AddBoardMemberCommand> _addBoardMemberValidator;
    private readonly ICurrentUser _currentUser;

    public BoardMembersController(ICommandHandler<AddBoardMemberCommand, BoardMember> addBoardMemberHandler,
        ICommandHandler<RemoveBoardMemberCommand> removeBoardMemberHandler,
        IQueryHandler<GetBoardMembersQuery, List<BoardMemberResponse>> getBoardMembersHandler,
        IValidator<AddBoardMemberCommand> addBoardMemberValidator, ICurrentUser currentUser)
    {
        _addBoardMemberHandler = addBoardMemberHandler;
        _removeBoardMemberHandler = removeBoardMemberHandler;
        _getBoardMembersHandler = getBoardMembersHandler;
        _addBoardMemberValidator = addBoardMemberValidator;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> GetBoardMembers(Guid boardId, CancellationToken ct = default)
    {
        var query = new GetBoardMembersQuery
        {
            BoardId = boardId,
            RequestingUserId = _currentUser.Id
        };
        var result = await _getBoardMembersHandler.Handle(query, ct);
        if (result.IsFailed)
            return NotFound(result.Errors);

        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> AddBoardMember(Guid boardId, [FromBody] AddBoardMemberRequest request, CancellationToken ct = default)
    {
        var command = new AddBoardMemberCommand
        {
            BoardId = boardId,
            UserIdToAdd = request.UserId,
            Role = request.Role,
            RequestingUserId = _currentUser.Id
        };

        var validationResult = await _addBoardMemberValidator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
            return BadRequest(new { Errors = validationResult.Errors.Select(e => e.ErrorMessage) });
        
        var result = await _addBoardMemberHandler.Handle(command, ct);
        if (result.IsFailed)
            return BadRequest(result.Errors);

        return NoContent();
    }

    
    [HttpDelete("{userId:guid}")]
    public async Task<IActionResult> RemoveBoardMember(Guid boardId, Guid userId, CancellationToken ct = default)
    {
        var command = new RemoveBoardMemberCommand
        {
            BoardId = boardId,
            UserIdToRemove = userId,
            RequestingUserId = _currentUser.Id
        };

        var result = await _removeBoardMemberHandler.Handle(command, ct);
        if (result.IsFailed)
            return BadRequest(result.Errors);

        return NoContent();
    }
}