using Application.Abstractions.CQRS;

namespace Application.Features.Media.Commands;

public sealed record DeleteMediaCommand : ICommand
{
    public required Guid EntityId { get; init; }
    public required Guid RequestingUserId { get; init; }
}