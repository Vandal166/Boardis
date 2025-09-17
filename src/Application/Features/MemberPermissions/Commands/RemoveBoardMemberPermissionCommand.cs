using Application.Abstractions.CQRS;
using Domain.Constants;

namespace Application.Features.MemberPermissions.Commands;

public sealed record RemoveBoardMemberPermissionCommand : ICommand
{
    public required Guid BoardId { get; init; }
    public required Guid MemberId { get; init; }
    public required Permissions Permission { get; init; }
    public required Guid RequestingUserId { get; init; }
}