namespace Application.DTOs.Boards;

public sealed class CreateBoardRequest
{
    public required string Title { get; init; }
    public string? Description { get; init; }
}