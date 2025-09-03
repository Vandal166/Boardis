using Application.Abstractions.CQRS;
using Application.DTOs.BoardLists;

namespace Application.Features.BoardLists.Queries;

public sealed record GetBoardListByIdQuery : IQuery<BoardListResponse>
{
    public required Guid BoardId { get; init; }
    public required Guid BoardListId { get; init; }
    public required Guid RequestingUserId { get; init; }
}