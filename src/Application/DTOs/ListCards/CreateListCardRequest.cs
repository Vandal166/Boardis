namespace Application.DTOs.ListCards;

public sealed class CreateListCardRequest
{
    public required string Title { get; init; }
    public string Description { get; init; } = string.Empty;
    public required int Position { get; init; }
}