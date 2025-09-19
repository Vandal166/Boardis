using Application.Abstractions.CQRS;
using Application.Contracts;
using Application.Contracts.Media;
using Application.Contracts.Persistence;
using Application.Features.Media.Commands;
using Domain.Constants;
using FluentResults;

namespace Application.Features.Media.CommandHandlers;

using Media = Domain.Images.Entities.Media;

internal sealed class UploadMediaCommandHandler : ICommandHandler<UploadMediaCommand, Media>
{
    private readonly IMediaRepository _mediaRepository;
    private readonly IBlobStorage _blobStorage;
    private readonly IUnitOfWork _unitOfWork;
    public UploadMediaCommandHandler(IMediaRepository mediaRepository, IBlobStorage blobStorage, IUnitOfWork unitOfWork)
    {
        _mediaRepository = mediaRepository;
        _blobStorage = blobStorage;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Media>> Handle(UploadMediaCommand command, CancellationToken ct = default)
    {
        var existingMedias = await _mediaRepository.GetByEntityIdAsync(command.EntityId, ct);
        if (existingMedias is not null && existingMedias.Count > 0)
        {
            foreach (var media in existingMedias)
            {
                await _blobStorage.DeleteFileAsync(media.Id, BlobContainers.MediaContainer, ct);
                await _mediaRepository.DeleteAsync(media, ct);
            }
        }
        var uploadResult = await _blobStorage.UploadFileAsync(command.File, command.File.ContentType, BlobContainers.MediaContainer, ct);
        if (uploadResult.IsFailed)
            return Result.Fail(uploadResult.Errors);

        var mediaResult = Media.Create(uploadResult.Value, command.EntityId, command.RequestingUserId);
        if (mediaResult.IsFailed)
            return Result.Fail(mediaResult.Errors);
        
        await _mediaRepository.AddAsync(mediaResult.Value, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        
        return Result.Ok(mediaResult.Value);
    }
}