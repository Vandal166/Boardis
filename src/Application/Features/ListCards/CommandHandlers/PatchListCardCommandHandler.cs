using Application.Abstractions.CQRS;
using Application.Contracts;
using Application.Features.ListCards.Commands;
using Domain.Contracts;
using FluentResults;

namespace Application.Features.ListCards.CommandHandlers;

internal sealed class PatchListCardCommandHandler : ICommandHandler<PatchCardCommand>
{
    private readonly IBoardRepository _boardRepository;
    private readonly IBoardListRepository _boardListRepository;
    private readonly IListCardRepository _listCardRepository;
    private readonly IUnitOfWork _unitOfWork;
    public PatchListCardCommandHandler(IBoardRepository boardRepository, IBoardListRepository boardListRepository,
        IListCardRepository listCardRepository, IUnitOfWork unitOfWork)
    {
        _boardRepository = boardRepository;
        _boardListRepository = boardListRepository;
        _listCardRepository = listCardRepository;
        _unitOfWork = unitOfWork;
    }
    
    
    public async Task<Result> Handle(PatchCardCommand command, CancellationToken ct = default)
    {
        var board = await _boardRepository.GetByIdAsync(command.BoardId, ct);
        if (board is null)
            return Result.Fail("Board not found");

        var listCard = await _listCardRepository.GetByIdAsync(command.CardId, ct);
        if (listCard is null || listCard.BoardListId != command.BoardListId)
            return Result.Fail("Card not found in the specified list");

        var list = await _boardListRepository.GetByIdAsync(command.BoardListId, ct);
        if (list is null || list.BoardId != command.BoardId)
            return Result.Fail("List not found in this board");
        
        var updateResult = listCard.Patch(command.Title, command.Position, command.Description, command.CompletedAt);
        if (updateResult.IsFailed)
            return Result.Fail(updateResult.Errors);
        
        await _listCardRepository.UpdateAsync(listCard, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        
        return Result.Ok();
    }
}