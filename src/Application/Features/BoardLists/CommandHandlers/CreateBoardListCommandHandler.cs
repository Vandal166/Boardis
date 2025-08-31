using Application.Abstractions.CQRS;
using Application.Contracts;
using Application.Features.BoardLists.Commands;
using Domain.Constants;
using Domain.Contracts;
using Domain.Entities;
using Domain.ValueObjects;
using FluentResults;

namespace Application.Features.BoardLists.CommandHandlers;

internal sealed class CreateBoardListCommandHandler : ICommandHandler<CreateBoardListCommand, BoardList>
{
    private readonly IBoardRepository _boardRepository;
    private readonly IBoardListRepository _boardListRepository;
    private readonly IUnitOfWork _unitOfWork;
    
    public CreateBoardListCommandHandler(IBoardRepository boardRepository, IBoardListRepository boardListRepository, IUnitOfWork unitOfWork)
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
        
        //permission check
        var boardMember = board.HasMember(command.RequestingUserId);
        if (boardMember is null)
            return Result.Fail<BoardList>("You are not a member of this board");
        
        if(!board.MemberHasRole(boardMember.UserId, Role.Create("Owner", "Owner").Value))
            return Result.Fail<BoardList>("You don't have permission to create a list in this board");
        
        var boardListResult = BoardList.Create(command.BoardId, command.Title);
        if (boardListResult.IsFailed)
            return Result.Fail<BoardList>(boardListResult.Errors);
        
        var boardList = boardListResult.Value;
        
        await _boardListRepository.AddAsync(boardList, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        
        return Result.Ok(boardList);
    }
}