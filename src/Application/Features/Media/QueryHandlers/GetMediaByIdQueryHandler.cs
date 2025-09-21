using Application.Abstractions.CQRS;
using Application.Contracts.Media;
using Application.Contracts.Persistence;
using Application.DTOs.Media;
using Application.Features.Media.Queries;
using Domain.Constants;
using FluentResults;
using Microsoft.AspNetCore.Http;

namespace Application.Features.Media.QueryHandlers;

internal sealed class GetMediaByIdQueryHandler : IQueryHandler<GetMediaByIdQuery, MediaResponse>
{
    private readonly IMediaRepository _mediaRepository;
    private readonly IBlobStorage _blobStorage;
    
    public GetMediaByIdQueryHandler(IMediaRepository mediaRepository, IBlobStorage blobStorage)
    {
        _mediaRepository = mediaRepository;
        _blobStorage = blobStorage;
    }
    
    public async Task<Result<MediaResponse>> Handle(GetMediaByIdQuery query, CancellationToken ct = default)
    {
        var media = await _mediaRepository.GetByIdAsync(query.MediaId, ct);
        if (media is null)
            return Result.Fail(new Error("MediaNotFound").WithMetadata("Status", StatusCodes.Status404NotFound));
        
        
        var fileResult = await _blobStorage.GetFileAsync(media.Id, BlobContainers.MediaContainer, ct);
        if (fileResult.IsFailed)
            return Result.Fail(fileResult.Errors);
        
        await using var memoryStream = new MemoryStream();
        await fileResult.Value.CopyToAsync(memoryStream, ct);
        await fileResult.Value.DisposeAsync();
        
        var mediaResponse = new MediaResponse
        {
            Id = media.Id,
            Data = memoryStream.ToArray(),
            UploadedAt = media.UploadedAt,
            UploadedByUserId = media.UploadedByUserId
        };
        
        
        return Result.Ok(mediaResponse);
    }
}