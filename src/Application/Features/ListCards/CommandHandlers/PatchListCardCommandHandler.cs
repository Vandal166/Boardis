using Application.Abstractions.CQRS;
using Application.Contracts;
using Application.Contracts.Board;
using Application.Features.ListCards.Commands;
using FluentResults;

namespace Application.Features.ListCards.CommandHandlers;

internal sealed class PatchListCardCommandHandler : ICommandHandler<PatchCardCommand>
{
    private readonly IBoardRepository _boardRepository;
    private readonly IUnitOfWork _unitOfWork;
    public PatchListCardCommandHandler(IBoardRepository boardRepository, IUnitOfWork unitOfWork)
    {
        _boardRepository = boardRepository;
        _unitOfWork = unitOfWork;
    }
    
    
    public async Task<Result> Handle(PatchCardCommand command, CancellationToken ct = default)
    {
        var board = await _boardRepository.GetWithCards(command.BoardId, ct);
        if (board is null)
            return Result.Fail("BoardNotFound");
        
        var boardList = board.GetListById(command.BoardListId);
        if(boardList is null)
            return Result.Fail("ListNotFound");
        
        var updateResult = boardList.PatchCard(command.CardId, command.RequestingUserId, command.Title, command.Position, command.Description, command.CompletedAt);
        if (updateResult.IsFailed)
            return Result.Fail(updateResult.Errors);
        
        await _unitOfWork.SaveChangesAsync(ct);
        
        return Result.Ok();
    }
}