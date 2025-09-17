using Application.Abstractions.CQRS;
using Domain.Common;

namespace Application.Features.BoardLists.Commands;

public sealed record PatchBoardListCommand : ICommand
{
    public Guid BoardId { get; init; }
    public Guid BoardListId { get; init; }
    public Guid RequestingUserId { get; init; }
    public PatchValue<string?> Title { get; init; }
    public PatchValue<double?> Position { get; init; }
    public PatchValue<int?> ColorArgb { get; init; }
}