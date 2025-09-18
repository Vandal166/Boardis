using Domain.Board.Events;
using Domain.BoardLists.Entities;
using Domain.BoardLists.Events;
using Domain.BoardMembers.Entities;
using Domain.BoardMembers.Events;
using Domain.Common;
using Domain.Constants;
using Domain.ValueObjects;
using FluentResults;

namespace Domain.Board.Entities;

public sealed class Board : Entity, IAggregateRoot
{
    public Guid Id { get; private set; }
    public Title Title { get; private set; }
    public string? Description { get; private set; }
    public Guid? WallpaperImageId { get; private set; } // optional image id in blob storage for the board
    
    public VisibilityLevel Visibility { get; private set; } // e.g. public, private
    
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    
    public IReadOnlyCollection<BoardList> Lists => _lists.AsReadOnly();
    private readonly List<BoardList> _lists = new();

    public IReadOnlyCollection<BoardMember> Members => _members.AsReadOnly();
    private readonly List<BoardMember> _members = new();
    
    private Board() { }
    
    public static Result<Board> Create(Title Title, string? Description, Guid? wallpaperImageId, Guid ownerId, VisibilityLevel visibility = VisibilityLevel.Private)
    {
        var board = new Board
        {
            Id = Guid.NewGuid(),
            Title = Title,
            Description = Description,
            WallpaperImageId = wallpaperImageId,
            Visibility = visibility,
            CreatedAt = DateTime.UtcNow
        };
        // invariants
        var addResult = board.AddMember(ownerId, Roles.OwnerId, ownerId);
        if (addResult.IsFailed)
            return Result.Fail<Board>(addResult.Errors);
        
        addResult.Value.AddPermission(Permissions.Create | Permissions.Read | Permissions.Update | Permissions.Delete, ownerId);
        
        board.AddDomainEvent(new BoardCreatedEvent(board.Id, ownerId));
        return Result.Ok(board);
    }
    
    
    public Result Patch(PatchValue<string?> title, PatchValue<string?> description, PatchValue<Guid?> wallpaperImageId, PatchValue<VisibilityLevel?> visibility, Guid requestedByUserId)
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
            if (descriptionResult.IsFailed) 
                errors.AddRange(descriptionResult.Errors);
        }

        if (wallpaperImageId.IsSet)
        {
            WallpaperImageId = wallpaperImageId.Value;
        }

        if (visibility.IsSet)
        {
            if (visibility.Value is null)
            {
                errors.Add(new Error("Visibility cannot be null.").WithMetadata("PropertyName", nameof(Visibility)));
            }
            else
            {
                Visibility = visibility.Value.Value;
            }
        }

        if (errors.Count != 0)
            return Result.Fail(errors);

        AddDomainEvent(new BoardUpdatedEvent(Id, requestedByUserId, Members.Select(g => g.UserId).ToList()));
        UpdatedAt = DateTime.UtcNow;
        return Result.Ok();
    }
    
    
    public Result<BoardList> AddList(string title, double position)
    {
        var listResult = BoardList.Create(Id, title, position);
        if (listResult.IsFailed)
            return Result.Fail<BoardList>(listResult.Errors);

        _lists.Add(listResult.Value);
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new BoardListAddedEvent(Id, listResult.Value.Id));
        
        return listResult;
    }

    public Result RemoveList(Guid listId)
    {
        var list = _lists.FirstOrDefault(l => l.Id == listId);
        if (list == null)
            return Result.Fail(new Error("List not found.").WithMetadata("Status", 404));

        _lists.Remove(list);
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new BoardListDeletedEvent(Id, listId));
        return Result.Ok();
    }
    
    public Result UpdateList(Guid listId, PatchValue<string?> title, PatchValue<double?> position, PatchValue<int?> colorArgb)
    {
        var list = _lists.FirstOrDefault(l => l.Id == listId);
        if (list == null)
            return Result.Fail(new Error("List not found.").WithMetadata("Status", 404));
        
        var patchResult = list.Patch(title, position, colorArgb);
        if (patchResult.IsFailed)
            return Result.Fail(patchResult.Errors);
        
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new BoardListUpdatedEvent(Id, listId));
        return Result.Ok();
    }

    public BoardList? GetListById(Guid listId) => _lists.FirstOrDefault(l => l.Id == listId);

    public Result Delete(Guid requestedByUserId)
    {
        if(_members.Any(m => m.RoleId == Roles.OwnerId && m.UserId != requestedByUserId))
            return Result.Fail(new Error("Only the owner can delete the board.").WithMetadata("Status", 403));
        
        AddDomainEvent(new BoardDeletedEvent(Id, requestedByUserId, Members.Select(g => g.UserId).ToList()));

        return Result.Ok();
    }
    
    public Result<BoardMember> AddMember(Guid userToAddId, Guid roleId, Guid requestingUserId)
    {
        if(GetMemberByUserId(userToAddId) is not null)
            return Result.Fail<BoardMember>(new Error("User is already a member of this board")
                .WithMetadata("Status", 409));
        
        var memberResult = BoardMember.Create(Id, userToAddId, roleId);
        if (memberResult.IsFailed)
            return Result.Fail<BoardMember>(memberResult.Errors);

        var permResult = memberResult.Value.AddPermission(Permissions.Read, requestingUserId);
        if (permResult.IsFailed)
            return Result.Fail(permResult.Errors);
        
        _members.Add(memberResult.Value);
        
        AddDomainEvent(new BoardMemberAddedEvent(Id, userToAddId, requestingUserId));
        UpdatedAt = DateTime.UtcNow;
        return memberResult;
    }

    public Result RemoveMember(Guid userId, Guid requestedByUserId)
    {
        var member = _members.FirstOrDefault(m => m.UserId == userId);
        if (member == null)
            return Result.Fail(new Error("Member not found.").WithMetadata("Status", 404));

        if (member.RoleId == Roles.OwnerId)
            return Result.Fail(new Error("Cannot remove the owner of the board.").WithMetadata("Status", 409));
        
        
        _members.Remove(member);
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new BoardMemberRemovedEvent(Id, userId, requestedByUserId));
        return Result.Ok();
    }

    public BoardMember? GetMemberByUserId(Guid userId) => _members.FirstOrDefault(m => m.UserId == userId);
    
    
    private Result UpdateTitle(string title)
    {
        var titleResult = Title.TryFrom(title);
        if (!titleResult.IsSuccess)
            return Result.Fail(titleResult.Error.ErrorMessage);

        Title = titleResult.ValueObject;
        return Result.Ok();
    }
    
    private Result UpdateDescription(string? description)
    {
        if (description is { Length: > 500 })
            return Result.Fail(new Error("Description cannot exceed 500 characters.").WithMetadata("PropertyName", nameof(Description)));

        Description = description;
        return Result.Ok();
    }
}