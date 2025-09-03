using Application.Abstractions.CQRS;
using Domain.Entities;

namespace Application.Features.BoardMembers.Commands;

public sealed record AddBoardMemberCommand : ICommand<BoardMember>
{
    public required Guid BoardId { get; init; }
    public required Guid UserIdToAdd { get; init; }
    public required string Role { get; init; }
    public required Guid RequestingUserId { get; init; }
}