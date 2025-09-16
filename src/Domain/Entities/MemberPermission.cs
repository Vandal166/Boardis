using Domain.Constants;
using FluentResults;

namespace Domain.Entities;

public sealed class MemberPermission
{
    public Permissions Permission { get; private set; }
    public DateTime GrantedAt { get; private set; }
    
    private MemberPermission() { }
    
    internal static Result<MemberPermission> Create(Permissions permission)
    {
        if (!Enum.IsDefined(permission))
            return Result.Fail<MemberPermission>(new Error("Invalid permission value").WithMetadata("PropertyName", nameof(Permission)));
        
        return Result.Ok(new MemberPermission
        {
            Permission = permission,
            GrantedAt = DateTime.UtcNow
        });
    }
}