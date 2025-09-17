using FluentResults;

namespace Domain.Common;

// Just an entity for stroing Named Roles like "Owner", "Member", etc.
public sealed class Role //TODO make this an Value Object
{
    public Guid Id { get; private set; }
    public string Key { get; private set; }
    
    private Role() { }
    
    public static Result<Role> Create(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return Result.Fail<Role>(new Error("Role name cannot be empty").WithMetadata("PropertyName", nameof(Key)));


        return Result.Ok(new Role
        {
            Key = char.ToUpper(key.Trim()[0]) + key.Trim().Substring(1).ToLower()
        });
    }
}