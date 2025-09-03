using ICommand = Application.Abstractions.CQRS.ICommand;

namespace Application.Features.BoardMembers.Commands;

public sealed record RemoveBoardMemberCommand : ICommand
{
    public required Guid BoardId { get; init; }
    public required Guid UserIdToRemove { get; init; }
    public required Guid RequestingUserId { get; init; }
}