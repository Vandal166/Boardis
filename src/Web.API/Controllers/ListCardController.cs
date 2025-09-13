using Application.Abstractions.CQRS;
using Application.Contracts.User;
using Application.DTOs.ListCards;
using Application.Features.ListCards.Commands;
using Application.Features.ListCards.Queries;
using Domain.Common;
using Domain.Constants;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
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
    private readonly ICommandHandler<PatchCardCommand> _patchCardHandler;
    private readonly ICurrentUser _currentUser;
    
    public ListCardController(
        ICommandHandler<CreateListCardCommand, ListCard> createListCardHandler,
        ICommandHandler<DeleteListCardCommand> deleteListCardHandler,
        IQueryHandler<GetListCardByIdQuery, ListCardResponse> getListCardByIdHandler,
        IQueryHandler<GetListCardsQuery, List<ListCardResponse>> getListCardsHandler,
        ICurrentUser currentUser, ICommandHandler<PatchCardCommand> patchCardHandler)
    {
        _createListCardHandler = createListCardHandler;
        _deleteListCardHandler = deleteListCardHandler;
        _getListCardByIdHandler = getListCardByIdHandler;
        _getListCardsHandler = getListCardsHandler;
        _currentUser = currentUser;
        _patchCardHandler = patchCardHandler;
    }

    [HttpGet("{cardId:guid}")]
    [HasPermission(Permissions.Read)]
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
    [HasPermission(Permissions.Read)]
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
    [HasPermission(Permissions.Create)]
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
    [HasPermission(Permissions.Delete)]
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
    
    [HttpPatch("{cardId:guid}")]
    [HasPermission(Permissions.Update)]
    [Consumes("application/json-patch+json")]
    public async Task<IActionResult> PatchCard(Guid boardId, Guid listId, Guid cardId, [FromBody] JsonPatchDocument<PatchCardRequest> patchDoc, CancellationToken ct = default)
    {
        if (patchDoc is null || patchDoc.Operations.Count == 0)
            return BadRequest("Invalid patch document.");
        
        var query = new GetListCardByIdQuery
        {
            BoardId = boardId,
            BoardListId = listId,
            CardId = cardId,
            RequestingUserId = _currentUser.Id
        };
        var currentCard = await _getListCardByIdHandler.Handle(query, ct);
        if (currentCard.IsFailed)
            return currentCard.ToProblemResponse(this, StatusCodes.Status404NotFound);

        var cardToPatch = new PatchCardRequest();
        
        // here the patch is applied, so cardToPatch now has the values from the patchDoc
        patchDoc.ApplyTo(cardToPatch, error =>
        {
            ModelState.AddModelError(error.AffectedObject?.ToString() ?? string.Empty, error.ErrorMessage);
        });
        if(!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var command = new PatchCardCommand
        {
            BoardId = boardId,
            BoardListId = listId,
            CardId = cardId,
            RequestingUserId = _currentUser.Id,
            
            Title = cardToPatch.HasProperty(nameof(cardToPatch.Title)) 
                ? PatchValue<string?>.Set(cardToPatch.Title) 
                : PatchValue<string?>.Unset(),
            Description = cardToPatch.HasProperty(nameof(cardToPatch.Description)) 
                ? PatchValue<string?>.Set(cardToPatch.Description) 
                : PatchValue<string?>.Unset(),
            Position = cardToPatch.HasProperty(nameof(cardToPatch.Position)) 
                ? PatchValue<double?>.Set(cardToPatch.Position) 
                : PatchValue<double?>.Unset(),
            CompletedAt = cardToPatch.HasProperty(nameof(cardToPatch.CompletedAt)) 
                ? PatchValue<DateTime?>.Set(cardToPatch.CompletedAt) 
                : PatchValue<DateTime?>.Unset()
        };
        
        var updateResult = await _patchCardHandler.Handle(command, ct);
        if (updateResult.IsFailed)
            return updateResult.ToProblemResponse(this);
        
        
        return NoContent();
    }
}