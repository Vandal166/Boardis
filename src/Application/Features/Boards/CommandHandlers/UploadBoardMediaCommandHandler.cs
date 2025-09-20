using Application.Abstractions.CQRS;
using Application.Contracts;
using Application.Contracts.Board;
using Application.Contracts.Media;
using Application.Contracts.Persistence;
using Application.Features.Boards.Commands;
using Domain.Board.Entities;
using Domain.Constants;
using FluentResults;
using Microsoft.AspNetCore.Http;

namespace Application.Features.Boards.CommandHandlers;

using Media = Domain.Images.Entities.Media;
internal sealed class UploadBoardMediaCommandHandler : ICommandHandler<UploadBoardMediaCommand, Board>
{
    private readonly IBoardRepository _boardRepository;
    private readonly IMediaRepository _mediaRepository;
    private readonly IBlobStorage _blobStorage;
    private readonly IUnitOfWork _unitOfWork;
    public UploadBoardMediaCommandHandler(IBoardRepository boardRepository, IMediaRepository mediaRepository, IBlobStorage blobStorage, IUnitOfWork unitOfWork)
    {
        _boardRepository = boardRepository;
        _mediaRepository = mediaRepository;
        _blobStorage = blobStorage;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Board>> Handle(UploadBoardMediaCommand command, CancellationToken ct = default)
    {
        var board = await _boardRepository.GetWithMembers(command.BoardId, ct);
        if (board is null)
            return Result.Fail(new Error("Board not found").WithMetadata("Status", StatusCodes.Status404NotFound));
        
        var oldWallpaperId = board.WallpaperImageId;
        var mediaId = Guid.NewGuid();
        
        var updateResult = board.SetWallpaperImageId(mediaId, command.RequestingUserId);
        if (updateResult.IsFailed)
            return Result.Fail(updateResult.Errors);
        
        var uploadResult = await _blobStorage.UploadFileAsync(mediaId, command.File, command.File.ContentType, BlobContainers.MediaContainer, ct);
        if (uploadResult.IsFailed)
            return Result.Fail(uploadResult.Errors);
        
        var mediaResult = Media.Create(uploadResult.Value, command.RequestingUserId);
        if (mediaResult.IsFailed)
            return Result.Fail(mediaResult.Errors);
        
        await _mediaRepository.AddAsync(mediaResult.Value, ct);
        
        if (oldWallpaperId is not null)
        {
            var oldMedia = await _mediaRepository.GetByIdAsync(oldWallpaperId.Value, ct);
            if (oldMedia is not null)
                await _mediaRepository.DeleteAsync(oldMedia, ct);
        }
        
        await _unitOfWork.SaveChangesAsync(ct);
        
        return Result.Ok(board);
    }
}