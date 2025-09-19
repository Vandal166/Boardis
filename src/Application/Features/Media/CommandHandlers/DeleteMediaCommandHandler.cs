using Application.Abstractions.CQRS;
using Application.Contracts;
using Application.Contracts.Media;
using Application.Contracts.Persistence;
using Application.Features.Media.Commands;
using Domain.Constants;
using FluentResults;
using Microsoft.AspNetCore.Http;

namespace Application.Features.Media.CommandHandlers;

internal sealed class DeleteMediaCommandHandler : ICommandHandler<DeleteMediaCommand>
{
    private readonly IMediaRepository _mediaRepository;
    private readonly IBlobStorage _blobStorage;
    private readonly IUnitOfWork _unitOfWork;
    public DeleteMediaCommandHandler(IMediaRepository mediaRepository, IBlobStorage blobStorage, IUnitOfWork unitOfWork)
    {
        _mediaRepository = mediaRepository;
        _blobStorage = blobStorage;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteMediaCommand command, CancellationToken ct = default)
    {
        var medias = await _mediaRepository.GetByEntityIdAsync(command.EntityId, ct);
        if (medias is null || medias.Count == 0)
            return Result.Fail(new Error("Media not found").WithMetadata("Status", StatusCodes.Status404NotFound));

        foreach (var media in medias)
        {
            await _blobStorage.DeleteFileAsync(media.Id, BlobContainers.MediaContainer, ct);
            await _mediaRepository.DeleteAsync(media, ct);
        }
        
        await _unitOfWork.SaveChangesAsync(ct);
        
        return Result.Ok();
    }
}