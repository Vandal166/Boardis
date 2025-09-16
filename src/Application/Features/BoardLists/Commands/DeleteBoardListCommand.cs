using Application.Abstractions.CQRS;

namespace Application.Features.BoardLists.Commands;

public sealed record DeleteBoardListCommand : ICommand, ICacheInvalidatingCommand
{
    public required Guid BoardId { get; init; }
    public required Guid BoardListId { get; init; }
    public required Guid RequestingUserId { get; init; }
    
    public IEnumerable<string> CacheKeysToInvalidate => new[] { $"lists_{BoardId}", $"cards_{BoardListId}" };
}