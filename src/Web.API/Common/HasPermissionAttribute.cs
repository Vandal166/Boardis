using Domain.Constants;
using Microsoft.AspNetCore.Authorization;

namespace Web.API.Common;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
internal sealed class HasPermissionAttribute : AuthorizeAttribute
{
    public HasPermissionAttribute(Permissions permissions)
        : base(policy: permissions.ToString()) {}
}