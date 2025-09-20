using Application.Abstractions.CQRS;
using Application.Contracts;
using Application.Contracts.Board;
using Application.Features.Boards.Commands;
using Domain.Board.Entities;
using Domain.ValueObjects;
using FluentResults;

namespace Application.Features.Boards.CommandHandlers;

internal sealed class CreateBoardCommandHandler : ICommandHandler<CreateBoardCommand, Board>
{
    private readonly IBoardRepository _boardRepo;
    private readonly IUnitOfWork _unitOfWork;
    public CreateBoardCommandHandler(IBoardRepository boardRepo, IUnitOfWork unitOfWork)
    {
        _boardRepo = boardRepo;
        _unitOfWork = unitOfWork;
    }
    
    
    public async Task<Result<Board>> Handle(CreateBoardCommand command, CancellationToken ct = default)
    {
        var titleResult = Title.TryFrom(command.Title);
        if (!titleResult.IsSuccess)
            return Result.Fail<Board>(titleResult.Error.ErrorMessage);
        
        var boardResult = Board.Create(titleResult.ValueObject, command.Description, command.OwnerId);
        if (boardResult.IsFailed)
            return Result.Fail<Board>(boardResult.Errors);

        var board = boardResult.Value;
        
        await _boardRepo.AddAsync(board, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        
        return Result.Ok(board);
    }
}