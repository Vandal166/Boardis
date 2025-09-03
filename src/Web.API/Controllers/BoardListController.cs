using Application.Abstractions.CQRS;
using Application.Contracts.User;
using Application.DTOs.BoardLists;
using Application.Features.BoardLists.Commands;
using Application.Features.BoardLists.Queries;
using Domain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.API.Controllers;

[ApiController][Route("api/boards/{boardId:guid}/lists")]
[Authorize]
public sealed class BoardListController : ControllerBase
{
    private readonly ICommandHandler<CreateBoardListCommand, BoardList> _createBoardListHandler;
    private readonly ICommandHandler<DeleteBoardListCommand> _deleteBoardListHandler;
    private readonly IQueryHandler<GetBoardListByIdQuery, BoardListResponse> _getBoardListByIdHandler;
    private readonly IQueryHandler<GetBoardListsQuery, List<BoardListResponse>> _getBoardListsHandler;
    private readonly IValidator<CreateBoardListCommand> _createBoardListValidator;
    private readonly ICurrentUser _currentUser;
    
    public BoardListController(ICommandHandler<CreateBoardListCommand, BoardList> createBoardListHandler,
        ICommandHandler<DeleteBoardListCommand> deleteBoardListHandler, IQueryHandler<GetBoardListByIdQuery, BoardListResponse> getBoardListByIdHandler,
        IQueryHandler<GetBoardListsQuery, List<BoardListResponse>> getBoardListsHandler, IValidator<CreateBoardListCommand> createBoardListValidator,
        ICurrentUser currentUser)
    {
        _createBoardListHandler = createBoardListHandler;
        _deleteBoardListHandler = deleteBoardListHandler;
        _getBoardListByIdHandler = getBoardListByIdHandler;
        _getBoardListsHandler = getBoardListsHandler;
        _createBoardListValidator = createBoardListValidator;
        _currentUser = currentUser;
    }

    //GET: api/boards/{boardId}/lists/{listId}
    [HttpGet("{listId:guid}")]
    public async Task<IActionResult> GetBoardListById(Guid boardId, Guid listId, CancellationToken ct = default)
    {
        var query = new GetBoardListByIdQuery
        {
            BoardId = boardId,
            BoardListId = listId,
            RequestingUserId = _currentUser.Id
        };
        var boardList = await _getBoardListByIdHandler.Handle(query, ct);
        if (boardList.IsFailed)
            return NotFound(boardList.Errors);
        
        return Ok(boardList.Value);
    }
    
    [HttpGet]
    public async Task<IActionResult> GetBoardLists(Guid boardId, CancellationToken ct = default)
    {
        var query = new GetBoardListsQuery
        {
            BoardId = boardId,
            RequestingUserId = _currentUser.Id
        };
        var result = await _getBoardListsHandler.Handle(query, ct);
        if (result.IsFailed)
            return NotFound(result.Errors);
        
        return Ok(result.Value);
    }
    
    [HttpPost]//[ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateBoardList(Guid boardId, [FromBody] CreateBoardListRequest request, CancellationToken ct = default)
    {
        var command = new CreateBoardListCommand
        {
            BoardId = boardId,
            Title = request.Title,
            RequestingUserId = _currentUser.Id
        };
        
        var validationResult = await _createBoardListValidator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
            return BadRequest(new {Errors = validationResult.Errors.Select(e => e.ErrorMessage)});
        
        var boardList = await _createBoardListHandler.Handle(command, ct);
        if (boardList.IsFailed)
            return BadRequest(boardList.Errors);
        
        return CreatedAtAction(nameof(GetBoardListById), new { boardId, listId = boardList.Value.Id }, boardList.Value);
    }


    [HttpDelete("{listId:guid}")]
    public async Task<IActionResult> DeleteBoardList(Guid boardId, Guid listId, CancellationToken ct = default)
    {
        var command = new DeleteBoardListCommand
        {
            BoardId = boardId,
            BoardListId = listId,
            RequestingUserId = _currentUser.Id
        };
        var result = await _deleteBoardListHandler.Handle(command, ct);
        if (result.IsFailed)
            return BadRequest(result.Errors);
        
        return NoContent();
    }
}