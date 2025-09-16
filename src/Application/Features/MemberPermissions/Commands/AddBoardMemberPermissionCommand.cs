using Application.Abstractions.CQRS;
using Domain.Constants;
using Domain.Entities;

namespace Application.Features.MemberPermissions.Commands;

public sealed record AddBoardMemberPermissionCommand : ICommand<MemberPermission>, ICacheInvalidatingCommand
{
    public required Guid BoardId { get; init; }
    public required Guid MemberId { get; init; }
    public required Permissions Permission { get; init; }
    public required Guid RequestingUserId { get; init; }
    
    public IEnumerable<string> CacheKeysToInvalidate => new[] { $"board_member_permissions_{BoardId}_{MemberId}" };
}