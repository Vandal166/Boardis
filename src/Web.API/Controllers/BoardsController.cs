using Application.Abstractions.CQRS;
using Application.Contracts.User;
using Application.DTOs.Boards;
using Application.Features.Boards.Commands;
using Domain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BoardsController : ControllerBase
{
    private readonly ICommandHandler<CreateBoardCommand, Board> _createBoardHandler;
    private readonly IValidator<CreateBoardCommand> _createBoardValidator;
    private readonly ICurrentUser _currentUser;
    
    public BoardsController(ICommandHandler<CreateBoardCommand, Board> createBoardHandler, IValidator<CreateBoardCommand> createBoardValidator, ICurrentUser currentUser)
    {
        _createBoardHandler = createBoardHandler;
        _createBoardValidator = createBoardValidator;
        _currentUser = currentUser;
    }
    
    [HttpGet("{boardId:guid}")]
    public async Task<IActionResult> GetBoardById(Guid boardId)
    {
        // Implementation to get board by ID
        return Ok();
    }

    [HttpPost]//[ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateBoard([FromBody] CreateBoardRequest request, CancellationToken ct = default)
    {
        if(!_currentUser.IsAuthenticated)
            return Unauthorized();
        
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
}