using Application.Abstractions.CQRS;
using Application.DTOs.BoardLists;

namespace Application.Features.BoardLists.Queries;

public sealed record GetBoardListsQuery : IQuery<List<BoardListResponse>>
{
    public required Guid BoardId { get; init; }
    public required Guid RequestingUserId { get; init; }
}