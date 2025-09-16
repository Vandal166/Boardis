using Application.Abstractions.CQRS;
using Domain.Common;
using Domain.Constants;
using Domain.ValueObjects;

namespace Application.Features.Boards.Commands;

public sealed record PatchBoardCommand : ICommand
{
    public Guid BoardId { get; init; }
    public Guid RequestingUserId { get; init; }
    public PatchValue<string?> Title { get; init; }
    public PatchValue<string?> Description { get; init; }
    public PatchValue<Guid?> WallpaperImageId { get; init; }
    public PatchValue<VisibilityLevel?> Visibility { get; init; }
}