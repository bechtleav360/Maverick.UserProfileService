using System;
using Maverick.Client.ArangoDb.Public;
using Maverick.Client.ArangoDb.Public.Exceptions;
using UserProfileService.Common.V2.Exceptions;

namespace UserProfileService.Adapter.Arango.V2.Extensions;

/// <summary>
///     Contains methods related to <see cref="Exception" />s.
/// </summary>
public static class ExceptionExtension
{
    private static ExceptionSeverity CreateSeverity(ApiErrorException apiErrorException)
    {
        return apiErrorException.GetSeverity() switch
        {
            AErrorSeverity.Error => ExceptionSeverity.Error,
            AErrorSeverity.Fatal => ExceptionSeverity.Fatal,
            AErrorSeverity.Hint => ExceptionSeverity.Hint,
            AErrorSeverity.Warning => ExceptionSeverity.Warning,
            _ => throw new ArgumentOutOfRangeException(nameof(AErrorSeverity))
        };
    }

    /// <summary>
    ///     Try to convert the given error to a database error if the error is of type <see cref="ApiErrorException" />.
    ///     Otherwise the error will be encapsulated in a normal error of type <see cref="Exception" />.
    /// </summary>
    /// <param name="exception">Error that should be tried to convert.</param>
    /// <param name="message">Optional message to be used for the error instead of the original error message.</param>
    /// <returns>
    ///     If the error is of type <see cref="ApiErrorException" /> error of type <see cref="DatabaseException" />,
    ///     otherwise <see cref="Exception" />.
    /// </returns>
    public static Exception ToDatabaseException(this Exception exception, string message = null)
    {
        ApiErrorException apiErrorException =
            exception as ApiErrorException ?? exception.InnerException as ApiErrorException;

        if (apiErrorException == null)
        {
            return message == null ? exception : new Exception(message, exception);
        }

        string concatenatedMessage = string.IsNullOrEmpty(message)
            ? $"{exception.Message} {apiErrorException.AMessage}"
            : $"{message} {apiErrorException.AMessage}";

        ExceptionSeverity severity = CreateSeverity(apiErrorException);

        return new DatabaseException(concatenatedMessage, exception, severity);
    }
}
