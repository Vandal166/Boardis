using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Mvc;

namespace Web.API.Common;

internal sealed class CustomAuthorizationMiddlewareResultHandler : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler _defaultHandler = new();
    
    public async Task HandleAsync(RequestDelegate next, HttpContext httpContext, AuthorizationPolicy policy, PolicyAuthorizationResult authorizeResult)
    {
        if (authorizeResult.AuthorizationFailure != null && authorizeResult.AuthorizationFailure.FailureReasons.Any())
        {
            var reasons = authorizeResult.AuthorizationFailure.FailureReasons.Select(r => r.Message).ToList();
            var problemDetails = new ProblemDetails
            {
                Title = "Access Denied",
                Status = StatusCodes.Status403Forbidden,
                Detail = string.Join("; ", reasons),
                Instance = httpContext.Request.Path
            };
            httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
            httpContext.Response.ContentType = "application/problem+json";
            await httpContext.Response.WriteAsJsonAsync(problemDetails);
            return; // Short-circuit
        }
        
        // Fallback to default
        await _defaultHandler.HandleAsync(next, httpContext, policy, authorizeResult);
    }
}