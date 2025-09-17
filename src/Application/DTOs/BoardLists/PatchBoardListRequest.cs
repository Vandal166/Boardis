using Application.DTOs.Common;

namespace Application.DTOs.BoardLists;

public sealed class PatchBoardListRequest : PatchRequest
{
    public string? Title
    {
        get => _title;
        set { _title = value; SetHasProperty(nameof(Title)); }
    }
    private string? _title;
    
    public double? Position
    {
        get => _position;
        set { _position = value; SetHasProperty(nameof(Position)); }
    }
    private double? _position;
    
    public int? ColorArgb
    {
        get => _colorArgb;
        set { _colorArgb = value; SetHasProperty(nameof(ColorArgb)); }
    }
    private int? _colorArgb;
}