using Application.Abstractions.CQRS;
using ICommand = Application.Abstractions.CQRS.ICommand;

namespace Application.Features.BoardMembers.Commands;

public sealed record RemoveBoardMemberCommand : ICommand, ICacheInvalidatingCommand
{
    public required Guid BoardId { get; init; }
    public required Guid UserIdToRemove { get; init; }
    public required Guid RequestingUserId { get; init; }
    
    public IEnumerable<string> CacheKeysToInvalidate => new[] { $"board_members_{BoardId}", $"board_{UserIdToRemove}" };
}