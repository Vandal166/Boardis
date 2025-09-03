using Application.Abstractions.CQRS;
using Application.Contracts.User;
using Application.DTOs.Boards;
using Application.Features.Boards.Commands;
using Application.Features.Boards.Queries;
using Domain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.API.Controllers;

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
        Console.WriteLine($"GetBoardById called by user {_currentUser.Id} for board {boardId}");
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

        /*return Ok(new BoardResponse
        {
            Id = new Guid("00000000-0000-0000-0000-000000000001"),
            Title = "Personal Board",
            Description = "This is your personal board."
        });*/
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