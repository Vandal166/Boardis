using Application.Abstractions.CQRS;
using Application.DTOs.ListCards;

namespace Application.Features.ListCards.Commands;

public sealed record UpdateListCardsOrderCommand : ICommand
{
    public required Guid BoardId { get; init; }
    public required Guid BoardListId { get; init; }
    public List<CardOrder> Cards { get; init; } = new();
    public required Guid RequestingUserId { get; init; }
}