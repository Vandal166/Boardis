using Application.Abstractions.CQRS;

namespace Application.Features.ListCards.Commands;

public sealed record DeleteListCardCommand : ICommand
{
    public required Guid BoardId { get; init; }
    public required Guid BoardListId { get; init; }
    public required Guid CardId { get; init; }
    public required Guid RequestingUserId { get; init; }
}