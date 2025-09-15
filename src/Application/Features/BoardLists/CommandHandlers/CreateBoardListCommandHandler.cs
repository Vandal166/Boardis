using Application.Abstractions.CQRS;
using Application.Contracts;
using Application.Contracts.Board;
using Application.Features.BoardLists.Commands;
using Domain.Entities;
using FluentResults;
using Microsoft.AspNetCore.Http;

namespace Application.Features.BoardLists.CommandHandlers;

internal sealed class CreateBoardListCommandHandler : ICommandHandler<CreateBoardListCommand, BoardList>
{
    private readonly IBoardRepository _boardRepository;
    private readonly IBoardListRepository _boardListRepository;
    private readonly IUnitOfWork _unitOfWork;
    
    public CreateBoardListCommandHandler(IBoardRepository boardRepository, IBoardListRepository boardListRepository, 
        IUnitOfWork unitOfWork)
    {
        _boardRepository = boardRepository;
        _boardListRepository = boardListRepository;
        _unitOfWork = unitOfWork;
    }
    
    
    public async Task<Result<BoardList>> Handle(CreateBoardListCommand command, CancellationToken ct = default)
    {
        var board = await _boardRepository.GetByIdAsync(command.BoardId, ct);
        if (board is null)
            return Result.Fail<BoardList>("Board not found");
        
        var existingList = await _boardListRepository.GetByBoardIdAsync(command.BoardId, ct);
        if (existingList is null || existingList.Any(l => l.Position == command.Position))
            return Result.Fail<BoardList>(new Error("A list in the same position already exists in this board. Reorder the existing lists first.")
                .WithMetadata("Status", StatusCodes.Status409Conflict));
        
        var boardListResult = BoardList.Create(command.BoardId, command.Title, command.Position);
        if (boardListResult.IsFailed)
            return Result.Fail<BoardList>(boardListResult.Errors);
        
        var boardList = boardListResult.Value;
        
        await _boardListRepository.AddAsync(boardList, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        
        return Result.Ok(boardList);
    }
}