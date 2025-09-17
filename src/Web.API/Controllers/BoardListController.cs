using Application.Abstractions.CQRS;
using Application.Contracts.User;
using Application.DTOs.BoardLists;
using Application.Features.BoardLists.Commands;
using Application.Features.BoardLists.Queries;
using Domain.BoardLists.Entities;
using Domain.Common;
using Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
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
    private readonly ICommandHandler<PatchBoardListCommand> _patchListHandler;
    private readonly ICurrentUser _currentUser;
    
    public BoardListController(ICommandHandler<CreateBoardListCommand, BoardList> createBoardListHandler,
        ICommandHandler<DeleteBoardListCommand> deleteBoardListHandler, IQueryHandler<GetBoardListByIdQuery, BoardListResponse> getBoardListByIdHandler,
        IQueryHandler<GetBoardListsQuery, List<BoardListResponse>> getBoardListsHandler, ICurrentUser currentUser, ICommandHandler<PatchBoardListCommand> patchListHandler)
    {
        _createBoardListHandler = createBoardListHandler;
        _deleteBoardListHandler = deleteBoardListHandler;
        _getBoardListByIdHandler = getBoardListByIdHandler;
        _getBoardListsHandler = getBoardListsHandler;
        _currentUser = currentUser;
        _patchListHandler = patchListHandler;
    }

    [HttpGet("{listId:guid}")]
    [HasPermission(Permissions.Read)]
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
    [HasPermission(Permissions.Read)]
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
    [HasPermission(Permissions.Create)]
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
    [HasPermission(Permissions.Delete)]
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
    
    [HttpPatch("{listId:guid}")]
    [HasPermission(Permissions.Update)]
    [Consumes("application/json-patch+json")]
    public async Task<IActionResult> PatchList(Guid boardId, Guid listId, [FromBody] JsonPatchDocument<PatchBoardListRequest> patchDoc, CancellationToken ct = default)
    {
        if (patchDoc is null || patchDoc.Operations.Count == 0)
            return BadRequest("Invalid patch document.");
        
        var query = new GetBoardListByIdQuery
        {
            BoardId = boardId,
            BoardListId = listId,
            RequestingUserId = _currentUser.Id
        };
        var currentList = await _getBoardListByIdHandler.Handle(query, ct);
        if (currentList.IsFailed)
            return currentList.ToProblemResponse(this, StatusCodes.Status404NotFound);

        var listToPatch = new PatchBoardListRequest();
        
        // here the patch is applied, so listToPatch now has the values from the patchDoc
        patchDoc.ApplyTo(listToPatch, error =>
        {
            ModelState.AddModelError(error.AffectedObject?.ToString() ?? string.Empty, error.ErrorMessage);
        });
        if(!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var command = new PatchBoardListCommand
        {
            BoardId = boardId,
            BoardListId = listId,
            RequestingUserId = _currentUser.Id,
            
            Title = listToPatch.HasProperty(nameof(listToPatch.Title)) 
                ? PatchValue<string?>.Set(listToPatch.Title) 
                : PatchValue<string?>.Unset(),
            Position = listToPatch.HasProperty(nameof(listToPatch.Position)) 
                ? PatchValue<double?>.Set(listToPatch.Position) 
                : PatchValue<double?>.Unset(),
            ColorArgb = listToPatch.HasProperty(nameof(listToPatch.ColorArgb)) 
                ? PatchValue<int?>.Set(listToPatch.ColorArgb) 
                : PatchValue<int?>.Unset()
        };
        
        var updateResult = await _patchListHandler.Handle(command, ct);
        if (updateResult.IsFailed)
            return updateResult.ToProblemResponse(this);
        
        
        return NoContent();
    }
}