namespace Application.DTOs.ListCards;

public sealed class CardOrder
{
    public required Guid CardId { get; init; }
    public required int Position { get; init; }
}