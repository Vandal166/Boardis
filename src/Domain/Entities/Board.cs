using Domain.Board.Events;
using Domain.BoardList.Events;
using Domain.Common;
using Domain.Constants;
using Domain.ValueObjects;
using FluentResults;

namespace Domain.Entities;

public sealed class Board : Entity, IAggregateRoot
{
    public Guid Id { get; private set; }
    public Title Title { get; private set; }
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
        
        addResult.Value.AddPermission(Permissions.Create | Permissions.Read | Permissions.Update | Permissions.Delete);
        
        board.AddDomainEvent(new BoardCreatedEvent(board.Id, ownerId));
        return Result.Ok(board);
    }
    
    
    public Result Patch(PatchValue<string?> title, PatchValue<string?> description, PatchValue<Guid?> wallpaperImageId, PatchValue<VisibilityLevel?> visibility)
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

        AddDomainEvent(new BoardUpdatedEvent(Id, Guid.Empty));
        UpdatedAt = DateTime.UtcNow;
        return Result.Ok();
    }
    
    
    public Result<BoardList> AddList(Title title, int position)
    {
        var listResult = BoardList.Create(Id, title, position);
        if (listResult.IsFailed)
            return Result.Fail<BoardList>(listResult.Errors);

        // invariants
        if (_boardLists.Any(l => l.Position == position))
            return Result.Fail<BoardList>(new Error("Position already occupied."));

        _boardLists.Add(listResult.Value);
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new BoardListAddedEvent(listResult.Value.Id, Id));
        
        return listResult;
    }

    public Result RemoveList(Guid listId)
    {
        var list = _boardLists.FirstOrDefault(l => l.Id == listId);
        if (list == null)
            return Result.Fail(new Error("List not found."));

        _boardLists.Remove(list);
        UpdatedAt = DateTime.UtcNow;
        return Result.Ok();
    }

    public BoardList? GetListById(Guid listId) => _boardLists.FirstOrDefault(l => l.Id == listId);

    public Result Delete(Guid requestedByUserId)
    {
        //TODO check if deletable etc
        AddDomainEvent(new BoardDeletedEvent(Id, requestedByUserId));

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

        var permResult = memberResult.Value.AddPermission(Permissions.Read);
        if (permResult.IsFailed)
            return Result.Fail(permResult.Errors);
        
        _members.Add(memberResult.Value);
        
        AddDomainEvent(new BoardMemberAddedEvent(Id, userToAddId, requestingUserId));
        UpdatedAt = DateTime.UtcNow;
        return memberResult;
    }

    public Result RemoveMember(Guid userId)
    {
        var member = _members.FirstOrDefault(m => m.UserId == userId);
        if (member == null)
            return Result.Fail(new Error("Member not found."));

        if (member.RoleId == Roles.OwnerId)
            return Result.Fail(new Error("Cannot remove the owner of the board."));
        
        
        _members.Remove(member);
        UpdatedAt = DateTime.UtcNow;
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