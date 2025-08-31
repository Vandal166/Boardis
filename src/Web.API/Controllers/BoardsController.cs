using Application.Abstractions.CQRS;
using Application.Contracts.User;
using Application.DTOs.BoardMembers;
using Application.DTOs.Boards;
using Application.Features.BoardMembers.Commands;
using Application.Features.Boards.Commands;
using Application.Features.Boards.Queries;
using Domain.Constants;
using Domain.Entities;
using Domain.ValueObjects;
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
    //private readonly ICommandHandler<RemoveBoardMemberCommand> _removeBoardMemberHandler;
    //private readonly IQueryHandler<GetBoardMembersQuery, List<BoardMemberResponse>> _getBoardMembersHandler;
    private readonly IValidator<AddBoardMemberCommand> _addBoardMemberValidator;
    private readonly ICurrentUser _currentUser;

    public BoardMembersController(ICommandHandler<AddBoardMemberCommand, BoardMember> addBoardMemberHandler,
        IValidator<AddBoardMemberCommand> addBoardMemberValidator,
        ICurrentUser currentUser)
    {
        _addBoardMemberHandler = addBoardMemberHandler;
        _addBoardMemberValidator = addBoardMemberValidator;
        _currentUser = currentUser;
    }
    /*public BoardMembersController(ICommandHandler<AddBoardMemberCommand, BoardMember> addBoardMemberHandler,
        ICommandHandler<RemoveBoardMemberCommand> removeBoardMemberHandler,
        IQueryHandler<GetBoardMembersQuery, List<BoardMemberResponse>> getBoardMembersHandler,
        ICurrentUser currentUser)
    {
        _addBoardMemberHandler = addBoardMemberHandler;
        _removeBoardMemberHandler = removeBoardMemberHandler;
        _getBoardMembersHandler = getBoardMembersHandler;
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
    }*/

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

    /*
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
    }*/
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class BoardsController : ControllerBase
{
    private readonly ICommandHandler<CreateBoardCommand, Board> _createBoardHandler;
    private readonly ICommandHandler<DeleteBoardCommand> _deleteBoardHandler;
    private readonly IQueryHandler<GetBoardByIdQuery, BoardResponse> _getBoardByIdHandler;
    private readonly IQueryHandler<GetBoardsQuery, List<BoardResponse>> _getBoardsHandler;
    private readonly IValidator<CreateBoardCommand> _createBoardValidator;
    private readonly ICurrentUser _currentUser;
    
    public BoardsController(ICommandHandler<CreateBoardCommand, Board> createBoardHandler,
        ICommandHandler<DeleteBoardCommand> deleteBoardHandler, IQueryHandler<GetBoardByIdQuery, BoardResponse> getBoardByIdHandler,
        IQueryHandler<GetBoardsQuery, List<BoardResponse>> getBoardsHandler, IValidator<CreateBoardCommand> createBoardValidator,
        ICurrentUser currentUser)
    {
        _createBoardHandler = createBoardHandler;
        _deleteBoardHandler = deleteBoardHandler;
        _getBoardByIdHandler = getBoardByIdHandler;
        _getBoardsHandler = getBoardsHandler;
        _createBoardValidator = createBoardValidator;
        _currentUser = currentUser;
    }
    
    [HttpGet("{boardId:guid}")]
    public async Task<IActionResult> GetBoardById(Guid boardId, CancellationToken ct = default)
    {
        var query = new GetBoardByIdQuery
        {
            BoardId = boardId,
            RequestingUserId = _currentUser.Id
        };
        var board = await _getBoardByIdHandler.Handle(query, ct);
        if (board.IsFailed)
            return NotFound(board.Errors);
        
        
        return Ok(board.Value);
    }
    
    [HttpGet]
    public async Task<IActionResult> GetBoards(CancellationToken ct = default)
    {
        var query = new GetBoardsQuery
        {
            RequestingUserId = _currentUser.Id
        };
        var result = await _getBoardsHandler.Handle(query, ct);
        if (result.IsFailed)
            return NotFound(result.Errors);
        
        return Ok(result.Value);
    }

    [HttpPost]//[ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateBoard([FromBody] CreateBoardRequest request, CancellationToken ct = default)
    {
        var command = new CreateBoardCommand
        {
            Title = request.Title,
            Description = request.Description,
            WallpaperImageId = request.WallpaperImageId,
            OwnerId = _currentUser.Id
        };
        
        var validationResult = await _createBoardValidator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
            return BadRequest(new {Errors = validationResult.Errors.Select(e => e.ErrorMessage)});
        
        var board = await _createBoardHandler.Handle(command, ct);
        if (board.IsFailed)
            return BadRequest(board.Errors);
        
        return CreatedAtAction(nameof(GetBoardById), new { boardId = board.Value.Id }, board.Value);
    }

    [HttpDelete("{boardId:guid}")]
    public async Task<IActionResult> DeleteBoard(Guid boardId, CancellationToken ct = default)
    {
        var command = new DeleteBoardCommand
        {
            BoardId = boardId,
            RequestingUserId = _currentUser.Id
        };
        
        var result = await _deleteBoardHandler.Handle(command, ct);
        if (result.IsFailed)
            return BadRequest(result.Errors);
        
        return NoContent();
    }
}