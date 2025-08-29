using Application.Abstractions.CQRS;
using Domain.Entities;

namespace Application.Features.Boards.Commands;

public sealed record CreateBoardCommand : ICommand<Board>
{
    public required string Title { get; init; }
    public string? Description { get; init; }
    public Guid? WallpaperImageId { get; init; }
    public required Guid OwnerId { get; init; }
}