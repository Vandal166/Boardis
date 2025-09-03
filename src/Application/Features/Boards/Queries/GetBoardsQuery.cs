using Application.Abstractions.CQRS;
using Application.DTOs.Boards;

namespace Application.Features.Boards.Queries;

public sealed record GetBoardsQuery : IQuery<List<BoardResponse>>
{
    public required Guid RequestingUserId { get; init; }
}