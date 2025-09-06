using Application.Abstractions.CQRS;
using Application.Contracts.User;
using Application.DTOs.ListCards;
using Application.Features.ListCards.Commands;
using Application.Features.ListCards.Queries;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.API.Common;

namespace Web.API.Controllers;

[ApiController]
[Route("api/boards/{boardId:guid}/lists/{listId:guid}/cards")]
[Authorize]
public sealed class ListCardController : ControllerBase
{
    private readonly ICommandHandler<CreateListCardCommand, ListCard> _createListCardHandler;
    private readonly ICommandHandler<DeleteListCardCommand> _deleteListCardHandler;
    private readonly IQueryHandler<GetListCardByIdQuery, ListCardResponse> _getListCardByIdHandler;
    private readonly IQueryHandler<GetListCardsQuery, List<ListCardResponse>> _getListCardsHandler;
    private readonly ICurrentUser _currentUser;
    
    public ListCardController(
        ICommandHandler<CreateListCardCommand, ListCard> createListCardHandler,
        ICommandHandler<DeleteListCardCommand> deleteListCardHandler,
        IQueryHandler<GetListCardByIdQuery, ListCardResponse> getListCardByIdHandler,
        IQueryHandler<GetListCardsQuery, List<ListCardResponse>> getListCardsHandler,
        ICurrentUser currentUser)
    {
        _createListCardHandler = createListCardHandler;
        _deleteListCardHandler = deleteListCardHandler;
        _getListCardByIdHandler = getListCardByIdHandler;
        _getListCardsHandler = getListCardsHandler;
        _currentUser = currentUser;
    }

    [HttpGet("{cardId:guid}")]
    public async Task<IActionResult> GetListCardById(Guid boardId, Guid listId, Guid cardId, CancellationToken ct = default)
    {
        var query = new GetListCardByIdQuery
        {
            BoardId = boardId,
            BoardListId = listId,
            CardId = cardId,
            RequestingUserId = _currentUser.Id
        };
        var boardList = await _getListCardByIdHandler.Handle(query, ct);
        if (boardList.IsFailed)
            return boardList.ToProblemResponse(this, StatusCodes.Status404NotFound);
        
        return Ok(boardList.Value);
    }
    
    [HttpGet]
    public async Task<IActionResult> GetListCards(Guid boardId, Guid listId, CancellationToken ct = default)
    {
        var query = new GetListCardsQuery
        {
            BoardId = boardId,
            BoardListId = listId,
            RequestingUserId = _currentUser.Id
        };
        var result = await _getListCardsHandler.Handle(query, ct);
        if (result.IsFailed)
            return result.ToProblemResponse(this, StatusCodes.Status404NotFound);
        
        return Ok(result.Value);
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateListCard(Guid boardId, Guid listId, [FromBody] CreateListCardRequest request, CancellationToken ct = default)
    {
        var command = new CreateListCardCommand
        {
            BoardId = boardId,
            BoardListId = listId,
            Title = request.Title,
            Description = request.Description,
            Position = request.Position,
            RequestingUserId = _currentUser.Id
        };
        
        var boardList = await _createListCardHandler.Handle(command, ct);
        if (boardList.IsFailed)
            return boardList.ToProblemResponse(this);
        
        return CreatedAtAction(nameof(GetListCardById), new { boardId, listId, cardId = boardList.Value.Id }, boardList.Value);
    }
    
    [HttpDelete("{cardId:guid}")]
    public async Task<IActionResult> DeleteListCard(Guid boardId, Guid listId, Guid cardId, CancellationToken ct = default)
    {
        var command = new DeleteListCardCommand
        {
            BoardId = boardId,
            BoardListId = listId,
            CardId = cardId,
            RequestingUserId = _currentUser.Id
        };
        
        var result = await _deleteListCardHandler.Handle(command, ct);
        if (result.IsFailed)
            return result.ToProblemResponse(this);
        
        return NoContent();
    }
}