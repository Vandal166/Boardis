using Application.Abstractions.CQRS;
using Application.Contracts.User;
using Application.DTOs.BoardLists;
using Application.Features.BoardLists.Commands;
using Application.Features.BoardLists.Queries;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.API.Common;

namespace Web.API.Controllers;

[ApiController]
[Route("api/boards/{boardId:guid}/lists")]
[Authorize]
public sealed class BoardListController : ControllerBase
{
    private readonly ICommandHandler<CreateBoardListCommand, BoardList> _createBoardListHandler;
    private readonly ICommandHandler<DeleteBoardListCommand> _deleteBoardListHandler;
    private readonly IQueryHandler<GetBoardListByIdQuery, BoardListResponse> _getBoardListByIdHandler;
    private readonly IQueryHandler<GetBoardListsQuery, List<BoardListResponse>> _getBoardListsHandler;
    private readonly ICommandHandler<UpdateBoardListCommand> _updateBoardListHandler;
    private readonly ICurrentUser _currentUser;
    
    public BoardListController(ICommandHandler<CreateBoardListCommand, BoardList> createBoardListHandler,
        ICommandHandler<DeleteBoardListCommand> deleteBoardListHandler, IQueryHandler<GetBoardListByIdQuery, BoardListResponse> getBoardListByIdHandler,
        IQueryHandler<GetBoardListsQuery, List<BoardListResponse>> getBoardListsHandler, ICurrentUser currentUser, ICommandHandler<UpdateBoardListCommand> updateBoardListHandler)
    {
        _createBoardListHandler = createBoardListHandler;
        _deleteBoardListHandler = deleteBoardListHandler;
        _getBoardListByIdHandler = getBoardListByIdHandler;
        _getBoardListsHandler = getBoardListsHandler;
        _currentUser = currentUser;
        _updateBoardListHandler = updateBoardListHandler;
    }

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
            return boardList.ToProblemResponse(this, StatusCodes.Status404NotFound);
        
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
            return result.ToProblemResponse(this, StatusCodes.Status404NotFound);
        
        return Ok(result.Value);
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateBoardList(Guid boardId, [FromBody] CreateBoardListRequest request, CancellationToken ct = default)
    {
        var command = new CreateBoardListCommand
        {
            BoardId = boardId,
            Title = request.Title,
            Position = request.Position,
            RequestingUserId = _currentUser.Id
        };
        
        var boardList = await _createBoardListHandler.Handle(command, ct);
        if (boardList.IsFailed)
            return boardList.ToProblemResponse(this);
        
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
            return result.ToProblemResponse(this);
        
        return NoContent();
    }


    [HttpPut("{listId:guid}")]
    public async Task<IActionResult> UpdateBoardList(Guid boardId, Guid listId, [FromBody] UpdateBoardListRequest request, CancellationToken ct = default)
    {
        var command = new UpdateBoardListCommand
        {
            BoardId = boardId,
            BoardListId = listId,
            Title = request.Title,
            Position = request.Position,
            ColorArgb = request.ColorArgb,
            RequestingUserId = _currentUser.Id
        };
        
        var result = await _updateBoardListHandler.Handle(command, ct);
        if (result.IsFailed)
            return result.ToProblemResponse(this);
        
        return NoContent();
    }
}