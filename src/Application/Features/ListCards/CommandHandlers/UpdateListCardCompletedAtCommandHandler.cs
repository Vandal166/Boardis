using Application.Abstractions.CQRS;
using Application.Contracts;
using Application.Features.ListCards.Commands;
using Domain.Contracts;
using Domain.ValueObjects;
using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;

namespace Application.Features.ListCards.CommandHandlers;

internal sealed class UpdateListCardCompletedAtCommandHandler : ICommandHandler<UpdateListCardCompletedAtCommand>
{
    private readonly IBoardRepository _boardRepository;
    private readonly IListCardRepository _listCardRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDistributedCache _cache;
    public UpdateListCardCompletedAtCommandHandler(IBoardRepository boardRepository,
        IListCardRepository listCardRepository, IUnitOfWork unitOfWork, IDistributedCache cache)
    {
        _boardRepository = boardRepository;
        _listCardRepository = listCardRepository;
        _unitOfWork = unitOfWork;
        _cache = cache;
    }
    
    
    public async Task<Result> Handle(UpdateListCardCompletedAtCommand command, CancellationToken ct = default)
    {
        var board = await _boardRepository.GetByIdAsync(command.BoardId, ct);
        if (board is null)
            return Result.Fail("Board not found");
        
        //permission check
        var boardMember = board.HasMember(command.RequestingUserId);
        if (boardMember is null)
            return Result.Fail(new Error("You are not a member of this board")
                .WithMetadata("Status", StatusCodes.Status403Forbidden));
        
        if(!board.MemberHasRole(boardMember.UserId, Role.Owner))
            return Result.Fail(new Error("You don't have permission to update a card in this list")
                .WithMetadata("Status", StatusCodes.Status403Forbidden));

        var listCard = await _listCardRepository.GetByIdAsync(command.CardId, ct);
        if (listCard is null || listCard.BoardListId != command.BoardListId)
            return Result.Fail("Card not found in the specified list");
        
        var updateResult = listCard.MarkAsCompleted(command.CompletedAt);
        if (updateResult.IsFailed)
            return Result.Fail(updateResult.Errors);
        
        await _listCardRepository.UpdateAsync(listCard, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        
        // Invalidate cache
        string cacheKey = $"cards_{command.BoardId}_{command.BoardListId}";
        await _cache.RemoveAsync(cacheKey, ct);
        
        return Result.Ok();
    }
}