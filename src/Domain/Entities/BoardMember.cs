using Domain.Constants;
using Domain.ValueObjects;
using FluentResults;

namespace Domain.Entities;

public sealed class BoardMember
{
    public Guid BoardId { get; private set; }
    public Guid UserId { get; private set; }
    public Role Role { get; private set; } 
    
    public DateTime JoinedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
   
    private BoardMember() { }
    
    public static Result<BoardMember> Create(Guid boardId, Guid userId, Role userBoardRole)
    {
        if (boardId == Guid.Empty)
            return Result.Fail<BoardMember>("Board ID cannot be empty.");
        
        if (userId == Guid.Empty)
            return Result.Fail<BoardMember>("User ID cannot be empty.");
        
        return Result.Ok(new BoardMember
        {
            BoardId = boardId,
            UserId = userId,
            Role = userBoardRole,
            JoinedAt = DateTime.UtcNow
        });
    }
    
    public Result UpdateRole(Role newRole, Guid requestingUserId)
    {
        //TODO Validate if requestingUserId has permission (e.g., is Owner)
        Role = newRole;
        UpdatedAt = DateTime.UtcNow;
        return Result.Ok();
    }
}