using Application.Abstractions.CQRS;
using Application.DTOs.ListCards;

namespace Application.Features.ListCards.Queries;

public sealed record GetListCardsQuery : IQuery<List<ListCardResponse>>
{
    public required Guid BoardId { get; init; }
    public required Guid BoardListId { get; init; }
    public required Guid RequestingUserId { get; init; }
}