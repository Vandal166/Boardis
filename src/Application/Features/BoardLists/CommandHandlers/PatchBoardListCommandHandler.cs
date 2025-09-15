using Application.Abstractions.CQRS;
using Application.Contracts;
using Application.Contracts.Board;
using Application.Features.BoardLists.Commands;
using Domain.Common;
using FluentResults;
using Microsoft.AspNetCore.Http;

namespace Application.Features.BoardLists.CommandHandlers;

internal sealed class PatchBoardListCommandHandler : ICommandHandler<PatchBoardListCommand>
{
    private readonly IBoardRepository _boardRepository;
    private readonly IBoardListRepository _boardListRepository;
    private readonly IUnitOfWork _unitOfWork;
    
    public PatchBoardListCommandHandler(IBoardRepository boardRepository, IBoardListRepository boardListRepository, 
        IUnitOfWork unitOfWork)
    {
        _boardRepository = boardRepository;
        _boardListRepository = boardListRepository;
        _unitOfWork = unitOfWork;
    }
    
    
    public async Task<Result> Handle(PatchBoardListCommand command, CancellationToken ct = default)
    {
        var board = await _boardRepository.GetByIdAsync(command.BoardId, ct);
        if (board is null)
            return Result.Fail("Board not found");

        var boardList = await _boardListRepository.GetByIdAsync(command.BoardListId, ct);
        if (boardList is null || boardList.BoardId != command.BoardId)
            return Result.Fail(new Error("List not found")
                .WithMetadata("Status", StatusCodes.Status404NotFound));
        
        if (command.Position.IsSet && command.Position.Value.HasValue)
        {
            var newPosition = command.Position.Value.Value;
            if (newPosition != boardList.Position)
            {
                var otherList = await _boardListRepository.GetByBoardIdAndPositionAsync(command.BoardId, newPosition, ct);
                if (otherList is not null && otherList.Id != boardList.Id)
                {
                    // Swap positions
                    var swapResult = otherList.Patch(new PatchValue<string?>(), PatchValue<int?>.Set(boardList.Position), new PatchValue<int?>());

                    if (swapResult.IsFailed)
                        return Result.Fail(swapResult.Errors);
                    
                    // The otherList is not tracked by default from GetByBoardIdAndPositionAsync, so we need to update it.
                    await _boardListRepository.UpdateAsync(otherList, ct);
                }
            }
        }
        
        var patchResult = boardList.Patch(command.Title, command.Position, command.ColorArgb);
        if (patchResult.IsFailed)
            return Result.Fail(patchResult.Errors);

        await _boardListRepository.UpdateAsync(boardList, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        
        return Result.Ok();
    }
}