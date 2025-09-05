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
    
    public static Result<BoardMember> Create(Guid BoardId, Guid UserId, Role userBoardRole)
    {
        var errors = new List<Error>();
        if (BoardId == Guid.Empty)
            errors.Add(new Error("Board ID cannot be empty.").WithMetadata("PropertyName", nameof(BoardId)));
        
        if (UserId == Guid.Empty)
            errors.Add(new Error("User ID cannot be empty.").WithMetadata("PropertyName", nameof(UserId)));
        
        if(errors.Count != 0)
            return Result.Fail<BoardMember>(errors);
        
        return Result.Ok(new BoardMember
        {
            BoardId = BoardId,
            UserId = UserId,
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