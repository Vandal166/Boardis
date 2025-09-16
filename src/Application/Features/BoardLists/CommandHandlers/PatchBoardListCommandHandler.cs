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
    private readonly IUnitOfWork _unitOfWork;
    
    public PatchBoardListCommandHandler(IBoardRepository boardRepository, IUnitOfWork unitOfWork)
    {
        _boardRepository = boardRepository;
        _unitOfWork = unitOfWork;
    }
    
    
    public async Task<Result> Handle(PatchBoardListCommand command, CancellationToken ct = default)
    {
        var board = await _boardRepository.GetWithLists(command.BoardId, ct);
        if (board is null)
            return Result.Fail("Board not found");
        
        var list = board.GetListById(command.BoardListId);
        if (list is null)
            return Result.Fail(new Error("List not found")
                .WithMetadata("Status", StatusCodes.Status404NotFound));
        
        var patchResult = list.Patch(command.Title, command.Position, command.ColorArgb);
        if (patchResult.IsFailed)
            return Result.Fail(patchResult.Errors);
        
        await _unitOfWork.SaveChangesAsync(ct);
        
        return Result.Ok();
    }
}