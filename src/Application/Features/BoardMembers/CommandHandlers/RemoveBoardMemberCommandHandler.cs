using Application.Abstractions.CQRS;
using Application.Contracts;
using Application.Features.BoardMembers.Commands;
using Domain.Contracts;
using Domain.ValueObjects;
using FluentResults;
using Microsoft.Extensions.Caching.Distributed;

namespace Application.Features.BoardMembers.CommandHandlers;

internal sealed class RemoveBoardMemberCommandHandler : ICommandHandler<RemoveBoardMemberCommand>
{
    private readonly IBoardRepository _boardRepository;
    private readonly IBoardMemberRepository _boardMemberRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDistributedCache _cache;
    public RemoveBoardMemberCommandHandler(IBoardRepository boardRepository, IBoardMemberRepository boardMemberRepository,
        IUnitOfWork unitOfWork, IDistributedCache cache)
    {
        _boardRepository = boardRepository;
        _boardMemberRepository = boardMemberRepository;
        _unitOfWork = unitOfWork;
        _cache = cache;
    }
    
    
    public async Task<Result> Handle(RemoveBoardMemberCommand command, CancellationToken ct = default)
    {
        var board = await _boardRepository.GetByIdAsync(command.BoardId, ct);
        if (board is null)
            return Result.Fail("Board not found");
        
        var removeResult = board.RemoveMember(command.UserIdToRemove, command.RequestingUserId);
        if (removeResult.IsFailed)
            return Result.Fail(removeResult.Errors);
        
        var member = await _boardMemberRepository.GetByIdAsync(command.BoardId, command.UserIdToRemove, ct);
        if (member is null)
            return Result.Fail("Board member not found");
        
        await _boardMemberRepository.DeleteAsync(member, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        
        // Invalidate cache
        string cacheKey = $"board_members_{command.BoardId}";
        await _cache.RemoveAsync(cacheKey, ct);
        
        return Result.Ok();
    }
}