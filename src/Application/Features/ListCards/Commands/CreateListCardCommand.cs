using Application.Abstractions.CQRS;
using Domain.Entities;

namespace Application.Features.ListCards.Commands;

public sealed record CreateListCardCommand : ICommand<ListCard>
{
    public required Guid BoardId { get; init; }
    public required Guid BoardListId { get; init; }
    public required string Title { get; init; }
    public string Description { get; init; } = string.Empty;
    public required int Position { get; init; }
    public required Guid RequestingUserId { get; init; }
}