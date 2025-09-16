using Application.Abstractions.CQRS;
using Domain.Common;

namespace Application.Features.BoardLists.Commands;

public sealed record PatchBoardListCommand : ICommand, ICacheInvalidatingCommand
{
    public Guid BoardId { get; init; }
    public Guid BoardListId { get; init; }
    public Guid RequestingUserId { get; init; }
    public PatchValue<string?> Title { get; init; }
    public PatchValue<int?> Position { get; init; }
    public PatchValue<int?> ColorArgb { get; init; }
    
    public IEnumerable<string> CacheKeysToInvalidate => new[] { $"lists_{BoardId}" };
}