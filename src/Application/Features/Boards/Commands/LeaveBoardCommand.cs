using Application.Abstractions.CQRS;

namespace Application.Features.Boards.Commands;

public sealed record LeaveBoardCommand : ICommand
{
    public required Guid BoardId { get; init; }
    public required Guid RequestingUserId { get; init; }
}