using Application.Abstractions.CQRS;
using Application.Contracts;
using Application.Contracts.Board;
using Application.Contracts.Media;
using Application.Features.Boards.Commands;
using FluentResults;

namespace Application.Features.Boards.CommandHandlers;

internal sealed class DeleteBoardCommandHandler : ICommandHandler<DeleteBoardCommand>
{
    private readonly IBoardRepository _boardRepository;
    private readonly IMediaRepository _mediaRepository;
    private readonly IUnitOfWork _unitOfWork;
    public DeleteBoardCommandHandler(IBoardRepository boardRepository, IMediaRepository mediaRepository, IUnitOfWork unitOfWork)
    {
        _boardRepository = boardRepository;
        _mediaRepository = mediaRepository;
        _unitOfWork = unitOfWork;
    }
    
    
    public async Task<Result> Handle(DeleteBoardCommand command, CancellationToken ct = default)
    {
        var board = await _boardRepository.GetWithMembers(command.BoardId, ct);
        if (board is null)
            return Result.Fail("Board not found");
        
        var deleteResult = board.DeleteBoard(command.RequestingUserId);
        if (deleteResult.IsFailed)
            return Result.Fail(deleteResult.Errors);
        
        if (board.WallpaperImageId is not null)
        {
            var oldMedia = await _mediaRepository.GetByIdAsync(board.WallpaperImageId.Value, ct);
            if (oldMedia is not null)
                await _mediaRepository.DeleteAsync(oldMedia, ct);
        }
        await _boardRepository.DeleteAsync(board, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        
        return Result.Ok();
    }
}