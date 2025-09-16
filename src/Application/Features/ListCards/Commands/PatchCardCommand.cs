using Application.Abstractions.CQRS;
using Domain.Common;
using Domain.ValueObjects;

namespace Application.Features.ListCards.Commands;

public sealed record PatchCardCommand : ICommand, ICacheInvalidatingCommand
{
    public Guid BoardId { get; init; }
    public Guid BoardListId { get; init; }
    public Guid CardId { get; init; }
    public Guid RequestingUserId { get; init; }
    public PatchValue<string?> Title { get; init; }
    public PatchValue<string?> Description { get; init; }
    public PatchValue<double?> Position { get; init; }
    public PatchValue<DateTime?> CompletedAt { get; init; }
    
    public IEnumerable<string> CacheKeysToInvalidate => new[] { $"cards_{BoardListId}" };
}