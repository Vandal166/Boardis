namespace Application.DTOs.BoardLists;

public sealed class UpdateBoardListRequest
{
    public required string Title { get; init; }
    public required int Position { get; init; }
    public required int ColorArgb { get; init; }
}