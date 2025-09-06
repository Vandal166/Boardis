using Application.Abstractions.CQRS;
using Application.Contracts;
using Application.Features.BoardLists.Commands;
using Application.Features.ListCards.Commands;
using Domain.Constants;
using Domain.Contracts;
using Domain.Entities;
using Domain.ValueObjects;
using FluentResults;
using Microsoft.AspNetCore.Http;

namespace Application.Features.ListCards.CommandHandlers;

internal sealed class CreateListCardCommandHandler : ICommandHandler<CreateListCardCommand, ListCard>
{
    private readonly IBoardRepository _boardRepository;
    private readonly IListCardRepository _listCardRepository;
    private readonly IUnitOfWork _unitOfWork;
    
    public CreateListCardCommandHandler(IBoardRepository boardRepository, IListCardRepository listCardRepository, IUnitOfWork unitOfWork)
    {
        _boardRepository = boardRepository;
        _listCardRepository = listCardRepository;
        _unitOfWork = unitOfWork;
    }
    
    
    public async Task<Result<ListCard>> Handle(CreateListCardCommand command, CancellationToken ct = default)
    {
        var board = await _boardRepository.GetByIdAsync(command.BoardId, ct);
        if (board is null)
            return Result.Fail<ListCard>("Board not found");
        
        //permission check
        var boardMember = board.HasMember(command.RequestingUserId);
        if (boardMember is null)
            return Result.Fail<ListCard>(new Error("You are not a member of this board")
                .WithMetadata("Status", StatusCodes.Status403Forbidden));
        
        if(!board.MemberHasRole(boardMember.UserId, Role.Owner))
            return Result.Fail<ListCard>(new Error("You don't have permission to create a card in this list")
                .WithMetadata("Status", StatusCodes.Status403Forbidden));

        var listCardResult = ListCard.Create(command.BoardListId, command.Title, command.Position, command.Description);
        if (listCardResult.IsFailed)
            return Result.Fail<ListCard>(listCardResult.Errors);
        
        await _listCardRepository.AddAsync(listCardResult.Value, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        
        return Result.Ok(listCardResult.Value);
    }
}