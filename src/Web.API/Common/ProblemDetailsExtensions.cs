using FluentResults;
using Microsoft.AspNetCore.Mvc;

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
        // Check if any errors have PropertyName metadata
        var isPropertySpecific = errors.Any(e => e.Metadata.ContainsKey("PropertyName"));
        int status = errors.SelectMany(e => e.Metadata)
            .Where(m => m.Key == "Status")
            .Select(m => Convert.ToInt32(m.Value))
            .FirstOrDefault();

        if (status == 0)
            status = defaultStatus;
        
        var problemType = GetProblemType(status);
        
        if (isPropertySpecific)
        {
            // Group errors by PropertyName for ValidationProblemDetails
            var errorDict = errors
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
                    g => g.Select(e => e.Message).ToArray()
                );

            var problems = new ValidationProblemDetails(errorDict)
            {
                Status = status,
                Title = "One or more validation errors occurred.",
                Type = problemType,
                Instance = controller.HttpContext.Request.Path
            };

            return new ObjectResult(problems) { StatusCode = status, Value = problems };
        }

        // Fallback for non-property-specific errors
        var problemDetails = new ProblemDetails
        {
            Status = status,
            Title = status == 404 ? "Resource not found." : "Operation failed.",
            Type = problemType,
            Instance = controller.HttpContext.Request.Path,
            Detail = string.Join("; ", errors.Select(e => e.Message))
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