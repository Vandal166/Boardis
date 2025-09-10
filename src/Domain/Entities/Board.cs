using Domain.Common;
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
    
    public static Result<Board> Create(string Title, string? Description, Guid? wallpaperImageId, VisibilityLevel visibility = VisibilityLevel.Private)
    {
        var errors = new List<Error>();
        if (string.IsNullOrWhiteSpace(Title))
            errors.Add(new Error("Title is required.").WithMetadata("PropertyName", nameof(Title)));
        
        if (Title.Length > 100)
            errors.Add(new Error("Title cannot exceed 100 characters.").WithMetadata("PropertyName", nameof(Title)));
        
        if (Description is { Length: > 500 })
            errors.Add(new Error("Description cannot exceed 500 characters.").WithMetadata("PropertyName", nameof(Description)));
        
        if (errors.Count != 0)
            return Result.Fail<Board>(errors);
        
        return Result.Ok(new Board
        {
            Id = Guid.NewGuid(),
            Title = Title,
            Description = Description,
            WallpaperImageId = wallpaperImageId,
            Visibility = visibility,
            CreatedAt = DateTime.UtcNow
        });
    }
    
    public Result Update(string Title, string? Description, Guid? wallpaperImageId, VisibilityLevel visibility)
    {
        var errors = new List<Error>();
        if (string.IsNullOrWhiteSpace(Title))
            errors.Add(new Error("Title is required.").WithMetadata("PropertyName", nameof(Title)));
        
        if (Title.Length > 100)
            errors.Add(new Error("Title cannot exceed 100 characters.").WithMetadata("PropertyName", nameof(Title)));
        
        if (Description is { Length: > 500 })
            errors.Add(new Error("Description cannot exceed 500 characters.").WithMetadata("PropertyName", nameof(Description)));
        
        if (errors.Count != 0)
            return Result.Fail(errors);
        
        this.Title = Title;
        this.Description = Description;
        this.WallpaperImageId = wallpaperImageId;
        this.Visibility = visibility;
        this.UpdatedAt = DateTime.UtcNow;
        
        return Result.Ok();
    }
    
    public Result Patch(
        PatchValue<string?> title,
        PatchValue<string?> description,
        PatchValue<Guid?> wallpaperImageId,
        PatchValue<VisibilityLevel?> visibility)
    {
        var errors = new List<IError>();

        if (title.IsSet)
        {
            if (title.Value is null)
            {
                errors.Add(new Error("Title cannot be null").WithMetadata("PropertyName", nameof(Title)));
            }
            else
            {
                var titleResult = UpdateTitle(title.Value);
                if (titleResult.IsFailed) 
                    errors.AddRange(titleResult.Errors);
            }
        }

        if (description.IsSet)
        {
            var descriptionResult = UpdateDescription(description.Value);
            if (descriptionResult.IsFailed) errors.AddRange(descriptionResult.Errors);
        }

        if (wallpaperImageId.IsSet)
        {
            var wallpaperResult = UpdateWallpaperImageId(wallpaperImageId.Value);
            if (wallpaperResult.IsFailed) errors.AddRange(wallpaperResult.Errors);
        }

        if (visibility.IsSet)
        {
            if (visibility.Value is null)
            {
                errors.Add(new Error("Visibility cannot be null.").WithMetadata("PropertyName", nameof(Visibility)));
            }
            else
            {
                var visibilityResult = UpdateVisibility(visibility.Value.Value);
                if (visibilityResult.IsFailed) 
                    errors.AddRange(visibilityResult.Errors);
            }
        }

        if (errors.Count != 0)
            return Result.Fail(errors);

        UpdatedAt = DateTime.UtcNow;
        return Result.Ok();
    }
    
    public Result<BoardMember> AddMember(Guid userIdToAdd, Role role, Guid requestingUserId)
    {
        //permission check
        var requestingMember = HasMember(requestingUserId);
        if (requestingMember is null)
            return Result.Fail<BoardMember>(new Error("You are not a member of this board")
                .WithMetadata("Status", 403));
        
        if(!MemberHasRole(requestingUserId, Role.Owner))
            return Result.Fail<BoardMember>(new Error("You don't have permission to add members to this board")
                .WithMetadata("Status", 403));
        
        if (HasMember(userIdToAdd) is not null)
            return Result.Fail<BoardMember>(new Error("User is already a member of this board")
                .WithMetadata("Status", 409));
        
        if(Equals(role, Role.Owner))
            return Result.Fail<BoardMember>(new Error("An owner is already assigned when the board is created. Cannot add another owner.")
                .WithMetadata("Status", 400));
        
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
            return Result.Fail(new Error("You are not a member of this board")
                .WithMetadata("Status", 403));
        
        if(!MemberHasRole(requestingUserId, Role.Owner))
            return Result.Fail(new Error("You don't have permission to remove members from this board")
                .WithMetadata("Status", 403));
        
        var memberToRemove = HasMember(userIdToRemove);
        if (memberToRemove is null)
            return Result.Fail(new Error("User is not a member of this board")
                .WithMetadata("Status", 409));
        
        if (Equals(memberToRemove.Role, Role.Owner))
            return Result.Fail(new Error("Cannot remove the owner of the board")
                .WithMetadata("Status", 400));
        
        _members.Remove(memberToRemove);
        UpdatedAt = DateTime.UtcNow;
        
        return Result.Ok();
    }
    
    private Result UpdateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Result.Fail(new Error("Title is required.").WithMetadata("PropertyName", nameof(Title)));
        if (title.Length > 100)
            return Result.Fail(new Error("Title cannot exceed 100 characters.").WithMetadata("PropertyName", nameof(Title)));
        Title = title;
        UpdatedAt = DateTime.UtcNow;
        return Result.Ok();
    }

    private Result UpdateDescription(string? description)
    {
        if (description is { Length: > 500 })
            return Result.Fail(new Error("Description cannot exceed 500 characters.").WithMetadata("PropertyName", nameof(Description)));
        Description = description;
        UpdatedAt = DateTime.UtcNow;
        return Result.Ok();
    }

    private Result UpdateWallpaperImageId(Guid? wallpaperImageId)
    {
        WallpaperImageId = wallpaperImageId;
        UpdatedAt = DateTime.UtcNow;
        return Result.Ok();
    }

    private Result UpdateVisibility(VisibilityLevel visibility)
    {
        Visibility = visibility;
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