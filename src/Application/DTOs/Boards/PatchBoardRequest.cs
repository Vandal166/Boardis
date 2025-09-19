using Application.DTOs.Common;
using Domain.Constants;

namespace Application.DTOs.Boards;

public sealed class PatchBoardRequest : PatchRequest
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

    public VisibilityLevel? Visibility
    {
        get => _visibility;
        set { _visibility = value; SetHasProperty(nameof(Visibility)); }
    }
    private VisibilityLevel? _visibility;
}