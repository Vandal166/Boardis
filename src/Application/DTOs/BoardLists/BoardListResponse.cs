namespace Application.DTOs.BoardLists;

public sealed record BoardListResponse
{
    public Guid Id { get; set; }
    public Guid BoardId { get; set; }
    public string Title { get; set; } = string.Empty;
    public double Position { get; set; }
    public int ColorArgb { get; set; }
}