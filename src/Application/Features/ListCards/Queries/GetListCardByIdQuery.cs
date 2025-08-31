using Application.Abstractions.CQRS;
using Application.DTOs.ListCards;

namespace Application.Features.ListCards.Queries;

public sealed record GetListCardByIdQuery : IQuery<ListCardResponse>
{
    public required Guid BoardId { get; init; }
    public required Guid BoardListId { get; init; }
    public required Guid CardId { get; init; }
    public required Guid RequestingUserId { get; init; }
}