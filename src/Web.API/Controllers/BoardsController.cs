using Application.Abstractions.CQRS;
using Application.Contracts.User;
using Application.DTOs.Boards;
using Application.Features.Boards.Commands;
using Application.Features.Boards.Queries;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.API.Common;

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
    private readonly ICurrentUser _currentUser;
    
    public BoardsController(ICommandHandler<CreateBoardCommand, Board> createBoardHandler,
        ICommandHandler<DeleteBoardCommand> deleteBoardHandler, IQueryHandler<GetBoardByIdQuery, BoardResponse> getBoardByIdHandler,
        IQueryHandler<GetBoardsQuery, List<BoardResponse>> getBoardsHandler,
        ICurrentUser currentUser)
    {
        _createBoardHandler = createBoardHandler;
        _deleteBoardHandler = deleteBoardHandler;
        _getBoardByIdHandler = getBoardByIdHandler;
        _getBoardsHandler = getBoardsHandler;
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
            return board.ToProblemResponse(this, StatusCodes.Status404NotFound);
        
        
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
            return result.ToProblemResponse(this, StatusCodes.Status404NotFound);

        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> CreateBoard([FromBody] CreateBoardRequest request, CancellationToken ct = default)
    {
        var command = new CreateBoardCommand
        {
            Title = request.Title,
            Description = request.Description,
            WallpaperImageId = request.WallpaperImageId,
            OwnerId = _currentUser.Id
        };
        
        var board = await _createBoardHandler.Handle(command, ct);
        if (board.IsFailed)
            return board.ToProblemResponse(this);
        
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
            return result.ToProblemResponse(this);
        
        return NoContent();
    }
}