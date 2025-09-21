using FluentResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Web.Resources.Resources.Boards;

namespace Web.API.Common;

internal sealed class CustomAuthorizationMiddlewareResultHandler : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler _defaultHandler = new();
    private readonly IStringLocalizer<BoardResources> _localizer;

    public CustomAuthorizationMiddlewareResultHandler(IStringLocalizer<BoardResources> localizer)
    {
        _localizer = localizer;
    }

    public async Task HandleAsync(RequestDelegate next, HttpContext httpContext, AuthorizationPolicy policy, PolicyAuthorizationResult authorizeResult)
    {
        if (authorizeResult.AuthorizationFailure != null && authorizeResult.AuthorizationFailure.FailureReasons.Any())
        {
            // localizing message (fallback to original if not a key)
            var localizedError =
                authorizeResult.AuthorizationFailure.FailureReasons.Select(r => _localizer[r.Message].Value).ToList();
            
            var problemDetails = new ProblemDetails
            {
                Title = "Access Denied",
                Status = StatusCodes.Status403Forbidden,
                Detail = string.Join("; ", localizedError),
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