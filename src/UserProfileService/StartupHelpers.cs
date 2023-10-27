using System.Diagnostics;
using System.Reflection;
using System.Runtime.Serialization;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Mvc;
using UserProfileService.Common.V2.Exceptions;
using UserProfileService.Queries.Language.ValidationException;
using UserProfileService.Validation.Abstractions;

namespace UserProfileService;

internal static class StartupHelpers
{
    /// <summary>
    ///     This activity should only created once on a central place and
    ///     is used for logging reason.
    /// </summary>
    internal static ActivitySource Source { get; set; } = new ActivitySource(
        "Maverick.UserProfileService",
        GetAssemblyVersion());

    internal static void ConfigureProblemDetails(ProblemDetailsOptions options)
    {
        // log all 5xx errors
        options.ShouldLogUnhandledException =
            (_, _, details) => details.Status is >= 500 and < 600;

        // include timestamp in output
        options.OnBeforeWriteDetails =
            (_, details) => details.Extensions.Add("Timestamp", DateTime.UtcNow);

        options.Map<InstanceNotFoundException>(
            (_, ex)
                => new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Not Found",
                    Detail = ex.Message,
                    Extensions =
                    {
                        { nameof(InstanceNotFoundException.Code), ex.Code },
                        { nameof(InstanceNotFoundException.RelatedId), ex.RelatedId }
                    }
                });

        // this handles ArgumentNullExceptions exceptions as well
        options.Map<ArgumentException>(
            (_, ex)
                => new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "One or more validation errors occurred",
                    Detail = ex.Message,
                    Extensions =
                    {
                        { nameof(ArgumentException.ParamName), ex.ParamName }
                    }
                });

        options.Map<AlreadyExistsException>(
            (_, ex)
                => new ProblemDetails
                {
                    Status = StatusCodes.Status409Conflict,
                    Title = "One or more validation errors occurred",
                    Detail = ex.Message,
                    Extensions =
                    {
                        { "ErrorType", "Functional" },
                        { "Show", true }
                    }
                });

        options.Map<SerializationException>(
            (_, ex)
                => new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Invalid filter",
                    Detail = ex.Message,
                    Extensions =
                    {
                        { "ProblemSection", ex.Data["value"] }
                    }
                });

        options.Map<ValidationException>(
            (_, ex)
                => new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "One or more validation errors occurred.",
                    Detail = ex.Message,
                    Extensions =
                    {
                        { "ValidationResults", ex.ValidationResults },
                        { "ErrorType", "Functional" },
                        { "Show", true }
                    }
                });

        options.Map<NotValidException>(
            (_, ex)
                => new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Invalid view filter",
                    Detail = ex.Message
                });

        options.Map<QueryValidationException>(
            (_, ex) => new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = ex.QuerySource,
                Detail = ex.Message
            });

        options.MapToStatusCode<NotImplementedException>(StatusCodes.Status501NotImplemented);
        options.MapToStatusCode<UnauthorizedAccessException>(StatusCodes.Status401Unauthorized);
        options.MapToStatusCode<HttpRequestException>(StatusCodes.Status503ServiceUnavailable);
        options.MapToStatusCode<Exception>(StatusCodes.Status500InternalServerError);
    }

    internal static string GetAssemblyVersion()
    {
        return Assembly.GetExecutingAssembly().GetName().Version?.ToString();
    }
}
