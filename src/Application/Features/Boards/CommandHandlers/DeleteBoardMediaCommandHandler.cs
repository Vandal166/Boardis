using Application.Abstractions.CQRS;
using Application.Contracts;
using Application.Contracts.Board;
using Application.Contracts.Media;
using Application.Features.Boards.Commands;
using Domain.Constants;
using FluentResults;
using Microsoft.AspNetCore.Http;

namespace Application.Features.Boards.CommandHandlers;

internal sealed class DeleteBoardMediaCommandHandler : ICommandHandler<DeleteBoardMediaCommand>
{
    private readonly IBoardRepository _boardRepository;
    private readonly IMediaRepository _mediaRepository;
    private readonly IUnitOfWork _unitOfWork;
    public DeleteBoardMediaCommandHandler(IBoardRepository boardRepository, IMediaRepository mediaRepository, IUnitOfWork unitOfWork)
    {
        _boardRepository = boardRepository;
        _mediaRepository = mediaRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteBoardMediaCommand command, CancellationToken ct = default)
    {
        var board = await _boardRepository.GetWithMembers(command.BoardId, ct);
        if (board is null)
            return Result.Fail(new Error("BoardNotFound").WithMetadata("Status", StatusCodes.Status404NotFound));
        
        var oldWallpaperId = board.WallpaperImageId;
        var deleteResult = board.DeleteWallpaperImageId(command.RequestingUserId);
        if (deleteResult.IsFailed)
            return Result.Fail(deleteResult.Errors);

        if (oldWallpaperId is not null)
        {
            var oldMedia = await _mediaRepository.GetByIdAsync(oldWallpaperId.Value, ct);
            if (oldMedia is not null)
                await _mediaRepository.DeleteAsync(oldMedia, ct);
        }
        
        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Ok();
    }
}