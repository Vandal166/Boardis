using Microsoft.AspNetCore.Authorization;

namespace Web.API.Common;

public class PermissionRequirement : IAuthorizationRequirement
{
    public PermissionRequirement(string permission)
    {
        Permission = permission;
    }
    
    public string Permission { get; }
}