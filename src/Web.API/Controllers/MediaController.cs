using Application.Abstractions.CQRS;
using Application.Contracts.User;
using Application.DTOs.Media;
using Application.Features.Media.Commands;
using Application.Features.Media.Queries;
using Domain.Constants;
using Domain.Images.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.API.Common;

namespace Web.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class MediaController : ControllerBase
{
    private readonly ICommandHandler<UploadMediaCommand, Media> _uploadMediaHandler;
    private readonly ICommandHandler<DeleteMediaCommand> _deleteMediaHandler;
    private readonly IQueryHandler<GetMediaByIdQuery, List<MediaResponse>> _getMediaByIdHandler;
    private readonly ICurrentUser _currentUser;
    
    public MediaController(ICommandHandler<UploadMediaCommand, Media> uploadMediaHandler,
        IQueryHandler<GetMediaByIdQuery, List<MediaResponse>> getMediaByIdHandler, ICurrentUser currentUser)
    {
        _uploadMediaHandler = uploadMediaHandler;
        _getMediaByIdHandler = getMediaByIdHandler;
        _currentUser = currentUser;
    }
    
    
    [HttpGet("{entityId:guid}")]
    public async Task<IActionResult> GetEntityMedia(Guid entityId, CancellationToken ct = default)
    {
        var query = new GetMediaByIdQuery
        {
            EntityId = entityId,
            RequestingUserId = _currentUser.Id
        };
        
        var media = await _getMediaByIdHandler.Handle(query, ct);
        if (media.IsFailed)
            return media.ToProblemResponse(this, StatusCodes.Status404NotFound);
        
        return Ok(media.Value);
    }
    
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadMedia([FromForm] UploadMediaRequest request, CancellationToken ct = default)
    {
        var command = new UploadMediaCommand
        {
            EntityId = request.EntityId,
            File = request.File,
            RequestingUserId = _currentUser.Id
        };

        var result = await _uploadMediaHandler.Handle(command, ct);
        if (result.IsFailed)
            return result.ToProblemResponse(this);

        return CreatedAtAction(nameof(GetEntityMedia), new { entityId = result.Value.BoundToEntityId }, result.Value);
    }

    [HttpDelete("{boardId:guid}")]
    [HasPermission(Permissions.Delete)]
    public async Task<IActionResult> DeleteBoardMedia(Guid boardId, CancellationToken ct = default)
    {
        var command = new DeleteMediaCommand
        {
            EntityId = boardId,
            RequestingUserId = _currentUser.Id
        };
        
        var result = await _deleteMediaHandler.Handle(command, ct);
        if (result.IsFailed)
            return result.ToProblemResponse(this);
        
        return NoContent();
    }
}