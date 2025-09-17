using Domain.Common;
using Domain.Constants;
using Domain.MemberPermissions.Entities;
using Domain.MemberPermissions.Events;
using FluentResults;

namespace Domain.BoardMembers.Entities;

public sealed class BoardMember : Entity
{
    public Guid BoardId { get; private set; }
    public Guid UserId { get; private set; }
    public Guid RoleId { get; private set; } // fk to Role(just for name)
    
    public DateTime JoinedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
   
    public IReadOnlyCollection<MemberPermission> Permissions => _permissions.AsReadOnly();
    private readonly List<MemberPermission> _permissions = new();
    
    private BoardMember() { }
    
    internal static Result<BoardMember> Create(Guid boardId, Guid userId, Guid roleId)
    {
        var errors = new List<Error>();
        if (boardId == Guid.Empty)
            errors.Add(new Error("Board ID cannot be empty.").WithMetadata("PropertyName", nameof(BoardId)));
        
        if (userId == Guid.Empty)
            errors.Add(new Error("User ID cannot be empty.").WithMetadata("PropertyName", nameof(UserId)));
        
        if (roleId == Guid.Empty)
            errors.Add(new Error("Assign an role to the member.").WithMetadata("PropertyName", nameof(RoleId)));
        
        if(errors.Count != 0)
            return Result.Fail<BoardMember>(errors);
        
        return Result.Ok(new BoardMember
        {
            BoardId = boardId,
            UserId = userId,
            RoleId = roleId,
            JoinedAt = DateTime.UtcNow
        });
    }
    
    public Result AddPermission(Permissions permission, Guid addedById)
    {
        var addedAny = false;
        var errors = new List<IError>();

        foreach (var perm in Enum.GetValues<Permissions>())
        {
            if (perm == Constants.Permissions.None)
                continue;
            if (permission.HasFlag(perm) && _permissions.All(p => p.Permission != perm))
            {
                var permResult = MemberPermission.Create(perm);
                if (permResult.IsFailed)
                {
                    errors.AddRange(permResult.Errors);
                    continue;
                }
                _permissions.Add(permResult.Value);
                addedAny = true;
            }
        }

        if (errors.Count > 0)
            return Result.Fail(errors);

        if (addedAny)
        {
            UpdatedAt = DateTime.UtcNow;
            AddDomainEvent(new MemberPermissionAddedEvent(BoardId, UserId, permission.ToString(), addedById));
            return Result.Ok();
        }

        return Result.Fail("No new permissions were added.");
    }
    
    public Result RemovePermission(Permissions permission, Guid removedById)
    {
        //if the permission does not exist
        if (_permissions.All(p => p.Permission != permission))
            return Result.Fail(new Error("The member does not have the specified permission.").WithMetadata("Status", 404));
        
        //if it's the last permission
        if (_permissions.Count == 1)
            return Result.Fail(new Error("Cannot remove the last permission from a member.").WithMetadata("Status", 409));
        
        _permissions.RemoveAll(p => p.Permission == permission);
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new MemberPermissionRemovedEvent(BoardId, UserId, permission.ToString(), removedById));
        return Result.Ok();
    }
}