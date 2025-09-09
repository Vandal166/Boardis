namespace Application.DTOs.ListCards;

public sealed class UpdateListCardRequest
{
    public required string Title { get; init; }
    public string Description { get; init; } = string.Empty;
    public required int Position { get; init; }
    public DateTime? CompletedAt { get; init; }
}