using Domain.Constants;
using FluentResults;

namespace Domain.Entities;

public sealed class Board
{
    public Guid Id { get; private set; }
    public string Title { get; private set; }
    public string? Description { get; private set; }
    public Guid? WallpaperImageId { get; private set; } // optional image id in blob storage for the board
    
    public VisibilityLevel Visibility { get; private set; } // e.g. public, private
    
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    
    public IReadOnlyCollection<BoardList> BoardLists => _boardLists.AsReadOnly();
    private readonly List<BoardList> _boardLists = new();

    public IReadOnlyCollection<BoardMember> Members => _members.AsReadOnly();
    private readonly List<BoardMember> _members = new();
    
    private Board() { }
    
    public static Result<Board> Create(string title, string? description, Guid? wallpaperImageId, VisibilityLevel visibility = VisibilityLevel.Private)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Result.Fail<Board>("Title cannot be empty.");
        
        if (title.Length > 100)
            return Result.Fail<Board>("Title cannot exceed 100 characters.");
        
        if (description is { Length: > 500 })
            return Result.Fail<Board>("Description cannot exceed 500 characters.");
        
        return Result.Ok(new Board
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = description,
            WallpaperImageId = wallpaperImageId,
            Visibility = visibility,
            CreatedAt = DateTime.UtcNow
        });
    }
    
    public Result SetVisibility(VisibilityLevel newVisibility, Guid requestingUserId)
    {
        // Only allow changing visibility if the requesting user is an owner
        var member = _members.FirstOrDefault(m => m.UserId == requestingUserId);
        if (member is null)
            return Result.Fail("User is not a member of the board.");
        
        if (member.Role != BoardRoles.Owner)
            return Result.Fail("Only board owners can change visibility.");
        
        Visibility = newVisibility;
        UpdatedAt = DateTime.UtcNow;
        
        return Result.Ok();
    }
}