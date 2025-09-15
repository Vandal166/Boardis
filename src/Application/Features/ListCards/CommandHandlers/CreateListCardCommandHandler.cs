using Application.Abstractions.CQRS;
using Application.Contracts;
using Application.Contracts.Board;
using Application.Features.ListCards.Commands;
using Domain.Entities;
using FluentResults;

namespace Application.Features.ListCards.CommandHandlers;

internal sealed class CreateListCardCommandHandler : ICommandHandler<CreateListCardCommand, ListCard>
{
    private readonly IBoardRepository _boardRepository;
    private readonly IBoardListRepository _boardListRepository;
    private readonly IListCardRepository _listCardRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateListCardCommandHandler(IBoardRepository boardRepository, IBoardListRepository boardListRepository,
        IListCardRepository listCardRepository, IUnitOfWork unitOfWork)
    {
        _boardRepository = boardRepository;
        _boardListRepository = boardListRepository;
        _listCardRepository = listCardRepository;
        _unitOfWork = unitOfWork;
    }
    
    
    public async Task<Result<ListCard>> Handle(CreateListCardCommand command, CancellationToken ct = default)
    {
        var board = await _boardRepository.GetByIdAsync(command.BoardId, ct);
        if (board is null)
            return Result.Fail<ListCard>("Board not found");
        
        var boardList = await _boardListRepository.GetByIdAsync(command.BoardListId, ct);
        if (boardList is null || boardList.BoardId != command.BoardId)
            return Result.Fail<ListCard>("List not found in this board");
        
        var listCardResult = ListCard.Create(command.BoardListId, command.Title, command.Position, command.Description);
        if (listCardResult.IsFailed)
            return Result.Fail<ListCard>(listCardResult.Errors);
        
        await _listCardRepository.AddAsync(listCardResult.Value, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        
        return Result.Ok(listCardResult.Value);
    }
}