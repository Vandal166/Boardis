using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Web.Resources.Resources.Boards;

namespace Web.API.Common;

public static class ProblemDetailsExtensions
{
    /// <summary>
    /// Converts a failed FluentResults Result (non-generic) to ProblemDetails.
    /// </summary>
    public static IActionResult ToProblemResponse(this Result result, ControllerBase controller, int defaultStatus = StatusCodes.Status400BadRequest) 
        => CreateProblemResponse(result.Errors, controller, defaultStatus);

    /// <summary>
    /// Converts a failed FluentResults Result generic to ProblemDetails.
    /// </summary>
    public static IActionResult ToProblemResponse<T>(this Result<T> result, ControllerBase controller, int defaultStatus = StatusCodes.Status400BadRequest)
        => CreateProblemResponse(result.Errors, controller, defaultStatus);

    private static IActionResult CreateProblemResponse(IReadOnlyList<IError> errors, ControllerBase controller, int defaultStatus)
    {
        // Resolve localizer from request services (runs in request context, so culture is set)
        var localizer = controller.HttpContext.RequestServices
            .GetRequiredService<IStringLocalizer<BoardResources>>();

        // Localize all messages (fallback to original if not a key)
        var localizedErrors = errors.Select(e =>
        {
            var localizedMessage = localizer[e.Message].Value;
            return new Error(localizedMessage)
                .WithMetadata(e.Metadata);  // Preserve metadata like PropertyName, Status
        }).ToList<IError>();

        // Now use localizedErrors instead of errors
        var isPropertySpecific = localizedErrors.Any(e => e.Metadata.ContainsKey("PropertyName"));
        int status = localizedErrors.SelectMany(e => e.Metadata)
            .Where(m => m.Key == "Status")
            .Select(m => Convert.ToInt32(m.Value))
            .FirstOrDefault();

        if (status == 0)
            status = defaultStatus;

        var problemType = GetProblemType(status);

        if (isPropertySpecific)
        {
            // Group and use localized messages
            var errorDict = localizedErrors
                .GroupBy(e =>
                {
                    if (!e.Metadata.TryGetValue("PropertyName", out var propNameObj) || propNameObj is not string propName)
                        return "General";

                    return propName.EndsWith(".Value")
                        ? propName.Substring(0, propName.Length - ".Value".Length)
                        : propName;
                })
                .ToDictionary(
                    g => g.Key ?? "General",
                    g => g.Select(e => e.Message).ToArray()  // Already localized
                );

            var problems = new ValidationProblemDetails(errorDict)
            {
                Status = status,
                Title = "One or more validation errors occurred.",  // Localize this too if needed: localizer["ValidationErrorsTitle"]
                Type = problemType,
                Instance = controller.HttpContext.Request.Path
            };

            return new ObjectResult(problems) { StatusCode = status, Value = problems };
        }

        // Fallback for non-property-specific errors
        var problemDetails = new ProblemDetails
        {
            Status = status,
            Title = status == 404 ? "Resource not found." : "Operation failed.",  // Localize if needed
            Type = problemType,
            Instance = controller.HttpContext.Request.Path,
            Detail = string.Join("; ", localizedErrors.Select(e => e.Message))  // Localized
        };

        return status switch
        {
            400 => controller.BadRequest(problemDetails),
            401 => controller.Unauthorized(problemDetails),
            404 => controller.NotFound(problemDetails),
            409 => controller.Conflict(problemDetails),
            _ => controller.StatusCode(status, problemDetails)
        };
    }

    private static string GetProblemType(int status) =>
        status switch
        {
            404 => "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            _ => "https://tools.ietf.org/html/rfc7807#section-3.1"
        };
}