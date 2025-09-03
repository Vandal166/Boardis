using Application.Abstractions.CQRS;
using Application.DTOs.Boards;

namespace Application.Features.Boards.Queries;

public sealed record GetBoardByIdQuery : IQuery<BoardResponse>
{
    public required Guid BoardId { get; init; }
    public required Guid RequestingUserId { get; init; }
}