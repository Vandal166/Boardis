using Application.Abstractions.CQRS;
using Application.Contracts.User;
using Application.DTOs.Media;
using Application.Features.Media.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.API.Common;

namespace Web.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class MediaController : ControllerBase
{
    private readonly IQueryHandler<GetMediaByIdQuery, MediaResponse> _getMediaByIdHandler;
    private readonly ICurrentUser _currentUser;
    
    public MediaController(IQueryHandler<GetMediaByIdQuery, MediaResponse> getMediaByIdHandler,
        ICurrentUser currentUser)
    {
        _getMediaByIdHandler = getMediaByIdHandler;
        _currentUser = currentUser;
    }
    
    
    [HttpGet("{mediaId:guid}")]
    public async Task<IActionResult> GetEntityMedia(Guid mediaId, CancellationToken ct = default)
    {
        var query = new GetMediaByIdQuery
        {
            MediaId = mediaId,
            RequestingUserId = _currentUser.Id
        };
        
        var media = await _getMediaByIdHandler.Handle(query, ct);
        if (media.IsFailed)
            return media.ToProblemResponse(this, StatusCodes.Status404NotFound);
        
        return Ok(media.Value);
    }
}