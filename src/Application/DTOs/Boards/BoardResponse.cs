using Domain.Constants;

namespace Application.DTOs.Boards;

public sealed class BoardResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public VisibilityLevel Visibility { get; set; }
}