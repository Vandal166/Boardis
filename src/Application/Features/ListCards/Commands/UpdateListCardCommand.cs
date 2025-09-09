using Application.Abstractions.CQRS;

namespace Application.Features.ListCards.Commands;

public sealed record UpdateListCardCommand : ICommand
{
    public required Guid BoardId { get; init; }
    public required Guid BoardListId { get; init; }
    public required Guid CardId { get; init; }
    public required string Title { get; init; }
    public string Description { get; init; } = string.Empty;
    public required int Position { get; init; }
    public DateTime? CompletedAt { get; init; }
    public required Guid RequestingUserId { get; init; }
}