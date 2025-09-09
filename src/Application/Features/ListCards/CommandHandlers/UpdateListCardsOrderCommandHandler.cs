using Application.Abstractions.CQRS;
using Application.Contracts;
using Application.Features.ListCards.Commands;
using Domain.Contracts;
using Domain.Entities;
using Domain.ValueObjects;
using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;

namespace Application.Features.ListCards.CommandHandlers;

internal sealed class UpdateListCardsOrderCommandHandler : ICommandHandler<UpdateListCardsOrderCommand>
{
    private readonly IBoardRepository _boardRepository;
    private readonly IBoardListRepository _boardListRepository;
    private readonly IListCardRepository _listCardRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDistributedCache _cache;
    public UpdateListCardsOrderCommandHandler(IBoardRepository boardRepository, IBoardListRepository boardListRepository,
        IListCardRepository listCardRepository, IUnitOfWork unitOfWork, IDistributedCache cache)
    {
        _boardRepository = boardRepository;
        _boardListRepository = boardListRepository;
        _listCardRepository = listCardRepository;
        _unitOfWork = unitOfWork;
        _cache = cache;
    }
    
    
    public async Task<Result> Handle(UpdateListCardsOrderCommand command, CancellationToken ct = default)
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

        var list = await _boardListRepository.GetByIdAsync(command.BoardListId, ct);
        if (list is null || list.BoardId != command.BoardId)
            return Result.Fail("List not found in this board");
        
        var allCards = await _listCardRepository.GetByBoardListIdAsync(command.BoardListId, ct);
        if(allCards is null || allCards.Any(c => c.BoardListId != command.BoardListId))
            return Result.Fail("Card not found in the specified list");
        
        var cardIds = allCards.Select(c => c.Id).ToHashSet(); // IDs from the database
        
        var requestedIds = command.Cards.Select(co => co.CardId).ToHashSet(); // IDs from the command
        if(requestedIds.Count == 1) // no need to update if only one card
            return Result.Ok();
        
        if (!requestedIds.All(id => cardIds.Contains(id))) // if any requested ID is not in the database
            return Result.Fail("One or more cards not found in the specified list");

        await using var transaction = await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            // temporary disabling unique constraints
            await _unitOfWork.ExecuteSqlRawAsync("SET CONSTRAINTS ALL DEFERRED", ct);
            
            var updatedCards = new List<ListCard>();
            foreach (var order in command.Cards)
            {
                var card = allCards.First(c => c.Id == order.CardId);
                var updateResult = card.Update(card.Title, order.Position, card.Description, card.CompletedAt);
                if (updateResult.IsFailed)
                    return Result.Fail(updateResult.Errors);
                updatedCards.Add(card);
            }

            // Bulk update all cards
            await _listCardRepository.UpdateRangeAsync(updatedCards, ct);

            await _unitOfWork.SaveChangesAsync(ct);

            await transaction.CommitAsync(ct);

            // Invalidate cache
            string cacheKey = $"cards_{command.BoardId}_{command.BoardListId}";
            await _cache.RemoveAsync(cacheKey, ct);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(ct);
            return Result.Fail(new Error("An error occurred while updating card order").CausedBy(ex));
        }
    }
}