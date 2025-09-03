using Domain.Constants;
using Domain.ValueObjects;
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
        var member = HasMember(requestingUserId);
        if (member is null)
            return Result.Fail("User is not a member of the board.");
        
        if(!MemberHasRole(requestingUserId, Role.Owner))
            return Result.Fail("Only board owners can change visibility.");
        
        Visibility = newVisibility;
        UpdatedAt = DateTime.UtcNow;
        
        return Result.Ok();
    }
    
    public Result<BoardMember> AddMember(Guid userIdToAdd, Role role, Guid requestingUserId)
    {
        //permission check
        var requestingMember = HasMember(requestingUserId);
        if (requestingMember is null)
            return Result.Fail<BoardMember>("You are not a member of this board");
        
        if(!MemberHasRole(requestingUserId, Role.Owner))
            return Result.Fail<BoardMember>("You don't have permission to add members to this board");
        
        if (HasMember(userIdToAdd) is not null)
            return Result.Fail<BoardMember>("User is already a member of this board");
        
        if(Equals(role, Role.Owner))
            return Result.Fail<BoardMember>("An owner is already assigned when the board is created. Cannot add another owner.");
        
        var memberResult = BoardMember.Create(Id, userIdToAdd, role);
        if (memberResult.IsFailed)
            return Result.Fail(memberResult.Errors);
        
        _members.Add(memberResult.Value);
        UpdatedAt = DateTime.UtcNow;
        
        return Result.Ok(memberResult.Value);
    }
    
    public Result RemoveMember(Guid userIdToRemove, Guid requestingUserId)
    {
        var requestingMember = HasMember(requestingUserId);
        if (requestingMember is null)
            return Result.Fail("You are not a member of this board");
        
        if(!MemberHasRole(requestingUserId, Role.Owner))
            return Result.Fail("You don't have permission to remove members from this board");
        
        var memberToRemove = HasMember(userIdToRemove);
        if (memberToRemove is null)
            return Result.Fail("User is not a member of this board");
        
        if (Equals(memberToRemove.Role, Role.Owner))
            return Result.Fail("Cannot remove the owner of the board");
        
        _members.Remove(memberToRemove);
        UpdatedAt = DateTime.UtcNow;
        
        return Result.Ok();
    }
    
    public bool HasVisibility(VisibilityLevel requiredVisibility) 
        => Visibility == requiredVisibility;
    
    public BoardMember? HasMember(Guid requestingUserId) 
        => _members.FirstOrDefault(m => m.UserId == requestingUserId);
    
    public bool MemberHasRole(Guid userId, Role role) 
        => _members.Any(m => m.UserId == userId && Equals(m.Role, role));
}