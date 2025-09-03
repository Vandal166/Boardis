using Application.Abstractions.CQRS;
using Application.Contracts;
using Application.Features.BoardMembers.Commands;
using Domain.Contracts;
using Domain.Entities;
using Domain.ValueObjects;
using FluentResults;

namespace Application.Features.BoardMembers.CommandHandlers;

internal sealed class AddBoardMemberCommandHandler : ICommandHandler<AddBoardMemberCommand, BoardMember>
{
    private readonly IBoardMemberRepository _boardMemberRepository;
    private readonly IBoardRepository _boardRepository;
    private readonly IUnitOfWork _unitOfWork;
    
    public AddBoardMemberCommandHandler(IBoardMemberRepository boardMemberRepository, IBoardRepository boardRepository,
        IUnitOfWork unitOfWork)
    {
        _boardMemberRepository = boardMemberRepository;
        _boardRepository = boardRepository;
        _unitOfWork = unitOfWork;
    }
    
    public async Task<Result<BoardMember>> Handle(AddBoardMemberCommand command, CancellationToken ct = default)
    {
        var board = await _boardRepository.GetByIdAsync(command.BoardId, ct);
        if (board is null)
            return Result.Fail<BoardMember>("Board not found");
        
        // Since the validator already checked the role exists, we can safely create it here
        var roleResult = Role.Create(command.Role, command.Role);
        if (roleResult.IsFailed)
            return Result.Fail<BoardMember>(roleResult.Errors);
      
        var newMemberResult = board.AddMember(command.UserIdToAdd, roleResult.Value, command.RequestingUserId);
        if (newMemberResult.IsFailed)
            return Result.Fail<BoardMember>(newMemberResult.Errors);
        
        var newMember = newMemberResult.Value;
        
        await _boardMemberRepository.AddAsync(newMember, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        
        return Result.Ok(newMember);
    }
}