using Application.Abstractions.CQRS;
using Application.Contracts;
using Application.Features.Boards.Commands;
using Domain.Constants;
using Domain.Contracts;
using Domain.Entities;
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
        // Validate Keycloak user
        //var userInfoResult = await _userService.GetUserInfoAsync(command.UserId); // or .UserExistsAsync instead
        //if (userInfoResult.IsFailed)
        //    return Result.Fail<Board>($"Invalid user: {userInfoResult.Errors.First().Message}");
        
        // Create board
        var boardResult = Board.Create(command.Title, command.Description, command.WallpaperImageId);
        if (boardResult.IsFailed)
            return Result.Fail<Board>(boardResult.Errors);

        var board = boardResult.Value;

        // Add owner as BoardMember
        var memberResult = BoardMember.Create(board.Id, command.OwnerId, BoardRoles.Owner);
        if (memberResult.IsFailed)
            return Result.Fail<Board>(memberResult.Errors);

        board.AddMember(memberResult.Value.UserId, memberResult.Value.Role, command.OwnerId);

        await _boardRepo.AddAsync(board, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        
        return Result.Ok(board);
    }
}