using Application.Abstractions.CQRS;
using Application.Contracts.User;
using Application.DTOs.Boards;
using Application.DTOs.Media;
using Application.Features.Boards.Commands;
using Application.Features.Boards.Queries;
using Domain.Board.Entities;
using Domain.Common;
using Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Web.API.Common;

namespace Web.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class BoardsController : ControllerBase
{
    private readonly ICommandHandler<CreateBoardCommand, Board> _createBoardHandler;
    private readonly ICommandHandler<UploadBoardMediaCommand, Board> _uploadMediaHandler;
    private readonly ICommandHandler<DeleteBoardCommand> _deleteBoardHandler;
    private readonly ICommandHandler<DeleteBoardMediaCommand> _deleteBoardMediaHandler;
    private readonly ICommandHandler<LeaveBoardCommand> _leaveBoardHandler;
    private readonly IQueryHandler<GetBoardByIdQuery, BoardResponse> _getBoardByIdHandler;
    private readonly IQueryHandler<GetBoardsQuery, List<BoardResponse>> _getBoardsHandler;
    private readonly ICommandHandler<PatchBoardCommand> _patchBoardHandler;
    private readonly ICurrentUser _currentUser;
    
    public BoardsController(ICommandHandler<CreateBoardCommand, Board> createBoardHandler,
        ICommandHandler<UploadBoardMediaCommand, Board> uploadMediaHandler,
        ICommandHandler<DeleteBoardCommand> deleteBoardHandler,
        ICommandHandler<DeleteBoardMediaCommand> deleteBoardMediaHandler,
        IQueryHandler<GetBoardByIdQuery, BoardResponse> getBoardByIdHandler,
        IQueryHandler<GetBoardsQuery, List<BoardResponse>> getBoardsHandler,
        ICommandHandler<PatchBoardCommand> patchBoardHandler,
        ICurrentUser currentUser, ICommandHandler<LeaveBoardCommand> leaveBoardHandler)
    {
        _createBoardHandler = createBoardHandler;
        _uploadMediaHandler = uploadMediaHandler;
        _deleteBoardHandler = deleteBoardHandler;
        _deleteBoardMediaHandler = deleteBoardMediaHandler;
        _getBoardByIdHandler = getBoardByIdHandler;
        _getBoardsHandler = getBoardsHandler;
        _patchBoardHandler = patchBoardHandler;
        _currentUser = currentUser;
        _leaveBoardHandler = leaveBoardHandler;
    }
    
    [HttpGet("{boardId:guid}")]
    [HasPermission(Permissions.Read)]
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
            OwnerId = _currentUser.Id
        };
        
        var board = await _createBoardHandler.Handle(command, ct);
        if (board.IsFailed)
            return board.ToProblemResponse(this);
        
        return CreatedAtAction(nameof(GetBoardById), new { boardId = board.Value.Id }, board.Value);
    }

    [HttpDelete("{boardId:guid}")]
    [HasPermission(Permissions.Delete)]
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
    
    [HttpPost("{boardId:guid}/leave")]
    public async Task<IActionResult> LeaveBoard(Guid boardId, CancellationToken ct = default)
    {
        var command = new LeaveBoardCommand
        {
            BoardId = boardId,
            RequestingUserId = _currentUser.Id
        };
        
        var result = await _leaveBoardHandler.Handle(command, ct);
        if (result.IsFailed)
            return result.ToProblemResponse(this);
        
        return NoContent();
    }
    
    
    [HttpPatch("{boardId:guid}")]
    [HasPermission(Permissions.Update)]
    [Consumes("application/json-patch+json")]
    public async Task<IActionResult> PatchBoard(Guid boardId, [FromBody] JsonPatchDocument<PatchBoardRequest> patchDoc, CancellationToken ct = default)
    {
        if (patchDoc is null || patchDoc.Operations.Count == 0)
            return BadRequest("Invalid patch document.");
       
        var boardToPatch = new PatchBoardRequest();
        
        // here the patch is applied, so boardToPatch now has the values from the patchDoc
        patchDoc.ApplyTo(boardToPatch, error =>
        {
            ModelState.AddModelError(error.AffectedObject?.ToString() ?? string.Empty, error.ErrorMessage);
        });
        if(!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var command = new PatchBoardCommand
        {
            BoardId = boardId,
            RequestingUserId = _currentUser.Id,
            Title = boardToPatch.HasProperty(nameof(boardToPatch.Title)) 
                ? PatchValue<string?>.Set(boardToPatch.Title) 
                : PatchValue<string?>.Unset(),
            Description = boardToPatch.HasProperty(nameof(boardToPatch.Description)) 
                ? PatchValue<string?>.Set(boardToPatch.Description) 
                : PatchValue<string?>.Unset(),
            Visibility = boardToPatch.HasProperty(nameof(boardToPatch.Visibility)) 
                ? PatchValue<VisibilityLevel?>.Set(boardToPatch.Visibility) 
                : PatchValue<VisibilityLevel?>.Unset()
        };
        
        var updateResult = await _patchBoardHandler.Handle(command, ct);
        if (updateResult.IsFailed)
            return updateResult.ToProblemResponse(this);
        
        
        return NoContent();
    }
    
    [HttpPost("{boardId:guid}/media")]
    [HasPermission(Permissions.Update)]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadBoardMedia(Guid boardId, [FromForm] UploadMediaRequest request, CancellationToken ct = default)
    {
        var command = new UploadBoardMediaCommand
        {
            BoardId = boardId,
            File = request.File,
            RequestingUserId = _currentUser.Id
        };

        var result = await _uploadMediaHandler.Handle(command, ct);
        if (result.IsFailed)
            return result.ToProblemResponse(this);

        return CreatedAtAction(nameof(GetBoardById), new { boardId = result.Value.Id }, result.Value);
    }

    [HttpDelete("{boardId:guid}/media")]
    [HasPermission(Permissions.Delete)]
    public async Task<IActionResult> DeleteBoardMedia(Guid boardId, CancellationToken ct = default)
    {
        var command = new DeleteBoardMediaCommand
        {
            BoardId = boardId,
            RequestingUserId = _currentUser.Id
        };

        var result = await _deleteBoardMediaHandler.Handle(command, ct);
        if (result.IsFailed)
            return result.ToProblemResponse(this);

        return NoContent();
    }
}