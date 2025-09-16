using Application.Abstractions.CQRS;
using Domain.Entities;

namespace Application.Features.BoardLists.Commands;

public sealed record CreateBoardListCommand : ICommand<BoardList>, ICacheInvalidatingCommand
{
    public required Guid BoardId { get; init; }
    public required string Title { get; init; }
    public required int Position { get; init; }
    public required Guid RequestingUserId { get; init; }
    
    
    public IEnumerable<string> CacheKeysToInvalidate => new[] { $"lists_{BoardId}" };
}