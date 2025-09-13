using System.Text.Json.Serialization;
using Domain.Constants;
using FluentResults;

namespace Domain.Entities;

public sealed class MemberPermission
{
    public Guid Id { get; private set; }
    public Guid BoardId { get; private set; } // Composite FK
    public Guid BoardMemberId { get; private set; }
    public Permissions Permission { get; private set; }

    public DateTime GrantedAt { get; private set; }
    
    private MemberPermission() { }
    
    public static Result<MemberPermission> Create(Guid boardId, Guid userId, Permissions permission)
    {
        if (boardId == Guid.Empty)
            return Result.Fail<MemberPermission>(new Error("Board ID cannot be empty").WithMetadata("PropertyName", nameof(BoardId)));
        
        if (userId == Guid.Empty)
            return Result.Fail<MemberPermission>(new Error("User ID cannot be empty").WithMetadata("PropertyName", nameof(BoardMemberId)));
        
        if (!Enum.IsDefined(permission))
            return Result.Fail<MemberPermission>(new Error("Invalid permission value").WithMetadata("PropertyName", nameof(Permission)));
        
        return Result.Ok(new MemberPermission
        {
            BoardId = boardId,
            BoardMemberId = userId,
            Permission = permission,
            GrantedAt = DateTime.UtcNow
        });
    }
    
    [JsonConstructor]
    private MemberPermission(Guid id, Guid boardId, Guid boardMemberId, Permissions permission, DateTime grantedAt)
    {
        Id = id;
        BoardId = boardId;
        BoardMemberId = boardMemberId;
        Permission = permission;
        GrantedAt = grantedAt;
    }
}