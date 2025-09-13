using Application.DTOs.Common;

namespace Application.DTOs.ListCards;

public sealed class PatchCardRequest : PatchRequest
{
    public string? Title
    {
        get => _title;
        set { _title = value; SetHasProperty(nameof(Title)); }
    }
    private string? _title;
    
    public string? Description
    {
        get => _description;
        set { _description = value; SetHasProperty(nameof(Description)); }
    }
    private string? _description;
    
    public double? Position
    {
        get => _position;
        set { _position = value; SetHasProperty(nameof(Position)); }
    }
    private double? _position;
    
    public DateTime? CompletedAt
    {
        get => _completedAt;
        set { _completedAt = value; SetHasProperty(nameof(CompletedAt)); }
    }
    private DateTime? _completedAt;
}