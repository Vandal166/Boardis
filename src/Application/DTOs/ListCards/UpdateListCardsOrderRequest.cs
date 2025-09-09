namespace Application.DTOs.ListCards;

public sealed class UpdateListCardsOrderRequest
{
    public List<CardOrder> Cards { get; init; } = new();
}