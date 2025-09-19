using Application.Abstractions.CQRS;
using Application.Contracts.Media;
using Application.Contracts.Persistence;
using Application.DTOs.Media;
using Application.Features.Media.Queries;
using Domain.Constants;
using FluentResults;
using Microsoft.AspNetCore.Http;

namespace Application.Features.Media.QueryHandlers;

internal sealed class GetMediaByIdQueryHandler : IQueryHandler<GetMediaByIdQuery, List<MediaResponse>>
{
    private readonly IMediaRepository _mediaRepository;
    private readonly IBlobStorage _blobStorage;
    
    public GetMediaByIdQueryHandler(IMediaRepository mediaRepository, IBlobStorage blobStorage)
    {
        _mediaRepository = mediaRepository;
        _blobStorage = blobStorage;
    }
    
    public async Task<Result<List<MediaResponse>>> Handle(GetMediaByIdQuery query, CancellationToken ct = default)
    {
        var medias = await _mediaRepository.GetByEntityIdAsync(query.EntityId, ct);
        if (medias is null || medias.Count == 0)
            return Result.Fail(new Error("Media not found").WithMetadata("Status", StatusCodes.Status404NotFound));
        
        var mediaResponses = new List<MediaResponse>();
        foreach (var media in medias)
        {
            var fileResult = await _blobStorage.GetFileAsync(media.Id, BlobContainers.MediaContainer, ct);
            if (fileResult.IsFailed)
                return Result.Fail(fileResult.Errors);
            
            await using var memoryStream = new MemoryStream();
            await fileResult.Value.CopyToAsync(memoryStream, ct);
            await fileResult.Value.DisposeAsync();
            
            var mediaResponse = new MediaResponse
            {
                Id = media.Id,
                BoundToEntityId = media.BoundToEntityId,
                Data = memoryStream.ToArray(),
                UploadedAt = media.UploadedAt,
                UploadedByUserId = media.UploadedByUserId
            };
            mediaResponses.Add(mediaResponse);
        }
        
        return Result.Ok(mediaResponses);
    }
}