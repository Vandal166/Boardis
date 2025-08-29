namespace Application.DTOs.Boards;

public sealed class CreateBoardRequest
{
    public string Title { get; set; }
    public string? Description { get; set; }
    public Guid? WallpaperImageId { get; set; }
}