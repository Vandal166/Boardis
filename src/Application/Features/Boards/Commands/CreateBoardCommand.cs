using Application.Abstractions.CQRS;
using Domain.Board.Entities;

namespace Application.Features.Boards.Commands;

public sealed record CreateBoardCommand : ICommand<Board>
{
    public required string Title { get; init; }
    public string? Description { get; init; }
    public required Guid OwnerId { get; init; }
}