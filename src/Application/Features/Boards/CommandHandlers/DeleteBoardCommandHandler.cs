using Application.Abstractions.CQRS;
using Application.Contracts;
using Application.Features.Boards.Commands;
using Domain.Constants;
using Domain.Contracts;
using Domain.ValueObjects;
using FluentResults;

namespace Application.Features.Boards.CommandHandlers;

internal sealed class DeleteBoardCommandHandler : ICommandHandler<DeleteBoardCommand>
{
    private readonly IBoardRepository _boardRepository;
    private readonly IUnitOfWork _unitOfWork;
    
    public DeleteBoardCommandHandler(IBoardRepository boardRepository, IUnitOfWork unitOfWork)
    {
        _boardRepository = boardRepository;
        _unitOfWork = unitOfWork;
    }
    
    
    public async Task<Result> Handle(DeleteBoardCommand command, CancellationToken ct = default)
    {
        var board = await _boardRepository.GetByIdAsync(command.BoardId, ct);
        if (board is null)
            return Result.Fail("Board not found");
        
        //permission check
        var boardMember = board.HasMember(command.RequestingUserId);
        if (boardMember is null)
            return Result.Fail("You are not a member of this board");
        
        if(!board.MemberHasRole(boardMember.UserId, Role.Owner))
            return Result.Fail("You don't have permission to delete this board");
        
        await _boardRepository.DeleteAsync(board, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        
        return Result.Ok();
    }
}