using Application.Abstractions.CQRS;
using Application.Contracts;
using Application.Contracts.Board;
using Application.Features.Boards.Commands;
using FluentResults;
using Microsoft.Extensions.Caching.Distributed;

namespace Application.Features.Boards.CommandHandlers;

internal sealed class DeleteBoardCommandHandler : ICommandHandler<DeleteBoardCommand>
{
    private readonly IBoardRepository _boardRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDistributedCache _cache;
    public DeleteBoardCommandHandler(IBoardRepository boardRepository, IUnitOfWork unitOfWork, IDistributedCache cache)
    {
        _boardRepository = boardRepository;
        _unitOfWork = unitOfWork;
        _cache = cache;
    }
    
    
    public async Task<Result> Handle(DeleteBoardCommand command, CancellationToken ct = default)
    {
        var board = await _boardRepository.GetByIdAsync(command.BoardId, ct);
        if (board is null)
            return Result.Fail("Board not found");
        
        await _boardRepository.DeleteAsync(board, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        
        // Invalidate cache
        string cacheKey = $"boards_{command.RequestingUserId}";
        await _cache.RemoveAsync(cacheKey, ct);
        
        return Result.Ok();
    }
}