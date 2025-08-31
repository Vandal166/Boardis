using Application.Abstractions.CQRS;
using Domain.Entities;

namespace Application.Features.BoardLists.Commands;

public sealed record CreateBoardListCommand : ICommand<BoardList>
{
    public required Guid BoardId { get; init; }
    public required string Title { get; init; }
    public required Guid RequestingUserId { get; init; }
}