using Application.Abstractions.CQRS;
using Domain.Entities;

namespace Application.Features.BoardMembers.Commands;

public sealed record AddBoardMemberCommand : ICommand<BoardMember>, ICacheInvalidatingCommand
{
    public required Guid BoardId { get; init; }
    public required Guid UserIdToAdd { get; init; }
    public required Guid RequestingUserId { get; init; }
    
    public IEnumerable<string> CacheKeysToInvalidate => new[] { $"board_members_{BoardId}", $"board_{UserIdToAdd}" };
}