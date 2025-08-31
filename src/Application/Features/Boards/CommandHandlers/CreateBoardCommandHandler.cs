using Application.Abstractions.CQRS;
using Application.Contracts;
using Application.Features.Boards.Commands;
using Domain.Constants;
using Domain.Contracts;
using Domain.Entities;
using Domain.ValueObjects;
using FluentResults;

namespace Application.Features.Boards.CommandHandlers;

internal sealed class CreateBoardCommandHandler : ICommandHandler<CreateBoardCommand, Board>
{
    private readonly IBoardRepository _boardRepo;
    private readonly IBoardMemberRepository _boardMemberRepo;
    private readonly IUnitOfWork _unitOfWork;
    
    public CreateBoardCommandHandler(IBoardRepository boardRepo, IBoardMemberRepository boardMemberRepo, IUnitOfWork unitOfWork)
    {
        _boardRepo = boardRepo;
        _boardMemberRepo = boardMemberRepo;
        _unitOfWork = unitOfWork;
    }
    
    
    public async Task<Result<Board>> Handle(CreateBoardCommand command, CancellationToken ct = default)
    {
        var boardResult = Board.Create(command.Title, command.Description, command.WallpaperImageId);
        if (boardResult.IsFailed)
            return Result.Fail<Board>(boardResult.Errors);

        var board = boardResult.Value;
        
        var memberResult = BoardMember.Create(board.Id, command.OwnerId, Role.Create("Owner", "Owner").Value); // self-adding as owner
        if (memberResult.IsFailed)
            return Result.Fail<Board>(memberResult.Errors);

        await _boardRepo.AddAsync(board, ct);
        await _boardMemberRepo.AddAsync(memberResult.Value, ct);
        
        await _unitOfWork.SaveChangesAsync(ct);
        
        return Result.Ok(board);
    }
}