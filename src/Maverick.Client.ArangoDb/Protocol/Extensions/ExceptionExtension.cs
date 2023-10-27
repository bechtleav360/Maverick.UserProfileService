using System;
using Maverick.Client.ArangoDb.Public;
using Maverick.Client.ArangoDb.Public.Annotations;
using Maverick.Client.ArangoDb.Public.Exceptions;

namespace Maverick.Client.ArangoDb.Protocol.Extensions;

/// <summary>
///     Extension methods for classes of type <see cref="Exception" />.
/// </summary>
public static class ExceptionExtension
{
    /// <summary>
    ///     Check if the exception is of type <see cref="ApiErrorException" />.
    /// </summary>
    /// <param name="exception">Exception to check.</param>
    /// <param name="arangoApiErrorException">Extracted <see cref="ApiErrorException" />.</param>
    /// <returns>True if error is <see cref="ApiErrorException" />, otherwise false.</returns>
    public static bool IsAError(this Exception exception, out ApiErrorException arangoApiErrorException)
    {
        arangoApiErrorException = exception as ApiErrorException ?? exception.InnerException as ApiErrorException;

        return arangoApiErrorException != null;
    }

    /// <summary>
    ///     Check if the exception is of type <see cref="ApiErrorException" /> and has severity
    ///     <see cref="AErrorSeverity.Error" />.
    /// </summary>
    /// <param name="exception">Exception to check.</param>
    /// <returns>True if error is <see cref="ApiErrorException" />, otherwise false.</returns>
    public static bool IsARetryError(this Exception exception)
    {
        if (exception.IsAError(out ApiErrorException apiErrorException))
        {
            AErrorSeverity? severity = apiErrorException.AErrorNum
                ?.GetAttributeOfType<ErrorClassificationAttribute>()
                ?.Severity;

            return severity is AErrorSeverity.Error;
        }

        return false;
    }

    /// <summary>
    ///     Check if the exception is of type <see cref="ApiErrorException" /> and has severity
    ///     <see cref="AErrorSeverity.Fatal" />.
    /// </summary>
    /// <param name="exception">Exception to check.</param>
    /// <returns>True if error is <see cref="ApiErrorException" />, otherwise false.</returns>
    public static bool IsAFatalError(this Exception exception)
    {
        if (exception.IsAError(out ApiErrorException apiErrorException))
        {
            AErrorSeverity? severity = apiErrorException.GetSeverity();

            return severity is AErrorSeverity.Fatal;
        }

        return false;
    }
}
