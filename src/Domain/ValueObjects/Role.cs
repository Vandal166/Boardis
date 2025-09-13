using System.Text.Json.Serialization;
using Domain.Constants;
using Domain.Entities;
using FluentResults;

namespace Domain.ValueObjects;

/*public sealed class Role
{
    /*public static readonly Role Owner = new Role("Owner", new List<Permissions>
        { Constants.Permissions.Create, Constants.Permissions.Delete, Constants.Permissions.Read, Constants.Permissions.Update});
    
    public static readonly Role Member = new Role("Member", new List<Permissions> 
        { Constants.Permissions.Read});#1#
    
    public string Key { get; private set; }
    
    public IReadOnlyCollection<BoardMemberPermissions> Permissions => _permissions.AsReadOnly();
    private readonly List<BoardMemberPermissions> _permissions = new();
    private Role() { }
    

    public static Result<Role> Create(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return Result.Fail<Role>("Role name cannot be empty");


        return Result.Ok(new Role
        {
            Key = char.ToUpper(key.Trim()[0]) + key.Trim().Substring(1).ToLower()
        });
    }
}*/
