using Domain.Constants;
using FluentResults;

namespace Domain.ValueObjects;

public sealed class Role
{
    public static readonly Role Owner = new Role("Owner", "Owner");
    public static readonly Role Member = new Role("Member", "Member");
    
    public string Key { get; } // Maps to Keycloak role name
    public string DisplayName { get; }

    private Role() { } // For EF Core

    private Role(string key, string displayName)
    {
        Key = key;
        DisplayName = displayName;
    }

    public static Result<Role> Create(string key, string displayName)
    {
        if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(displayName))
            return Result.Fail<Role>("Role name cannot be empty");

        return Result.Ok(new Role
        (
            char.ToUpper(key.Trim()[0]) + key.Trim().Substring(1).ToLower(),
            char.ToUpper(displayName[0]) + displayName.Substring(1).ToLower()
        ));
    }

    // For equality comparison (EF Core)
    public override bool Equals(object? obj) => obj is Role role && Key.Equals(role.Key, StringComparison.OrdinalIgnoreCase);
    public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(Key);
}
