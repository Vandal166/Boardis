using Application.Abstractions.CQRS;
using Domain.Common;

namespace Application.Features.ListCards.Commands;

public sealed record PatchCardCommand : ICommand
{
    public Guid BoardId { get; init; }
    public Guid BoardListId { get; init; }
    public Guid CardId { get; init; }
    public Guid RequestingUserId { get; init; }
    public PatchValue<string?> Title { get; init; }
    public PatchValue<string?> Description { get; init; }
    public PatchValue<double?> Position { get; init; }
    public PatchValue<DateTime?> CompletedAt { get; init; }
}