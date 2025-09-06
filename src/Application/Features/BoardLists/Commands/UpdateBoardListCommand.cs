using Application.Abstractions.CQRS;

namespace Application.Features.BoardLists.Commands;

public sealed record UpdateBoardListCommand : ICommand
{
    public required Guid BoardId { get; init; }
    public required Guid BoardListId { get; init; }
    public required string Title { get; init; }
    public required int Position { get; init; }
    public required int ColorArgb { get; init; }
    public required Guid RequestingUserId { get; init; }
}