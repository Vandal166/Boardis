using Application.Abstractions.CQRS;
using Application.Contracts;
using Application.Contracts.Board;
using Application.Features.BoardLists.Commands;
using Domain.BoardLists.Entities;
using Domain.ValueObjects;
using FluentResults;

namespace Application.Features.BoardLists.CommandHandlers;

internal sealed class CreateBoardListCommandHandler : ICommandHandler<CreateBoardListCommand, BoardList>
{
    private readonly IBoardRepository _boardRepository;
    private readonly IUnitOfWork _unitOfWork;
    public CreateBoardListCommandHandler(IBoardRepository boardRepository, IUnitOfWork unitOfWork)
    {
        _boardRepository = boardRepository;
        _unitOfWork = unitOfWork;
    }
    
    
    public async Task<Result<BoardList>> Handle(CreateBoardListCommand command, CancellationToken ct = default)
    {
        var board = await _boardRepository.GetWithCards(command.BoardId, ct);
        if (board is null)
            return Result.Fail<BoardList>("Board not found");
        
        var addListResult = board.AddList(command.Title, command.Position);
        if (addListResult.IsFailed)
            return Result.Fail<BoardList>(addListResult.Errors);
        
        var boardList = addListResult.Value;
        //TODO no need to check if pos occupied, AddList does it
        // after board.AddList(...)

        await _unitOfWork.SaveChangesAsync(ct);
        
        return Result.Ok(boardList);
    }
}