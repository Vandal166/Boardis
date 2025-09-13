namespace Application.DTOs.ListCards;

public sealed record ListCardResponse
{
    public Guid Id { get; set; }
    public Guid BoardId { get; set; }
    public Guid BoardListId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; } = string.Empty;
    public DateTime? CompletedAt { get; set; }
    public double Position { get; set; }
}