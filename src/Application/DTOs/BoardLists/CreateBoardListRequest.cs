namespace Application.DTOs.BoardLists;

public sealed class CreateBoardListRequest
{
    public required string Title { get; init; }
    public required int Position { get; init; } // position/order of the list on the board
}