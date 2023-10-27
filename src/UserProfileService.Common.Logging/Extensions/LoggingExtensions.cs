using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace UserProfileService.Common.Logging.Extensions;

/// <summary>
///     Logging Extension class for simply the logging.
///     Every logging method will log the calling member name too.
/// </summary>
public static class LoggingExtensions
{
    private static void RunLoggerSafely(Action action, ILogger logger, string message, string caller, int arguments)
    {
        try
        {
            action();
        }
        catch (Exception ex)
        {
            string escapedMessage = message.Replace('{', '[').Replace('}', ']');

            var errorMessage =
                $"Some error occurred while logging a message. Problem appear in method {caller}. Logging message was: {escapedMessage}. Number of arguments that have to be escaped:{arguments}.";

            logger.LogWarning(ex, errorMessage);
        }
    }

    /// <summary>
    ///     Calls <see cref="EnterMethod" /> when called, and returns an <see cref="IDisposable" />
    ///     that calls <see cref="ExitMethod" /> when disposed.
    /// </summary>
    /// <param name="logger">The given logger.</param>
    /// <param name="caller">The method name where the logger is called.</param>
    /// <returns>An <see cref="IDisposable" /> that calls <see cref="ExitMethod" /> when disposed.</returns>
    /// <example>
    ///     Usage:
    ///     <code>
    ///     private readonly ILogger _Logger;
    /// 
    ///     public void MyMethod()
    ///     {
    ///         using var _ = _Logger.MethodScope();
    /// 
    ///         // some code...
    /// 
    ///         // _.Dispose() implicitly called when MyMethod is exited
    ///     }
    /// </code>
    /// </example>
    public static IDisposable MethodScope(this ILogger logger, [CallerMemberName] string caller = null)
    {
        logger.EnterMethod(caller);

        return new MethodScopeToken(logger, caller);
    }

    /// <summary>
    ///     Use this method, when you enter it.
    /// </summary>
    /// <param name="logger">The given logger.</param>
    /// <param name="caller">The method name where the logger is called.</param>
    public static void EnterMethod(this ILogger logger, [CallerMemberName] string caller = null)
    {
        logger?.LogTrace("Enter method {caller}().", caller);
    }

    /// <summary>
    ///     Use this method, when you exit it.
    /// </summary>
    /// <param name="logger">The given logger.</param>
    /// <param name="caller">The method name where the logger is called.</param>
    public static void ExitMethod(this ILogger logger, [CallerMemberName] string caller = null)
    {
        logger?.LogTrace("Exit method {caller}().", caller);
    }

    /// <summary>
    ///     Use this method, when you exit it and this method will return the provided value.
    /// </summary>
    /// <param name="logger">The given logger.</param>
    /// <param name="resultingObject">Returns the resulting object.</param>
    /// <param name="caller">The method name where the logger is called.</param>
    public static TResult ExitMethod<TResult>(
        this ILogger logger,
        TResult resultingObject,
        [CallerMemberName] string caller = null)
    {
        logger?.LogTrace("Exit method {caller}().", caller);

        return resultingObject;
    }

    /// <summary>
    ///     Uses the LogDebug method of <see cref="LoggerExtensions" /> with additional information
    ///     for better logging.
    /// </summary>
    /// <param name="logger">The given logger.</param>
    /// <param name="message">The message pattern which should be used to build the log message.</param>
    /// <param name="caller">The method name where the logger is called.</param>
    /// <param name="arguments">Arguments to pass to the logger.</param>
    public static void LogDebugMessage(
        this ILogger logger,
        string message,
        object[] arguments,
        [CallerMemberName] string caller = null)
    {
        RunLoggerSafely(
            () =>
            {
                message = $"{{caller}}(): {message}";
                logger?.LogDebug(message, new object[] { caller }.Concat(arguments).ToArray());
            },
            logger,
            message,
            caller,
            arguments.Length);
    }

    /// <summary>
    ///     Uses the LogDebug method of <see cref="LoggerExtensions" /> with additional information
    ///     for better logging.
    /// </summary>
    /// <param name="logger">The given logger.</param>
    /// <param name="message">The message pattern which should be used to build the log message.</param>
    /// <param name="ex">Exception which should be logged and not thrown.</param>
    /// <param name="caller">The method name where the logger is called.</param>
    /// <param name="arguments">Arguments to pass to the logger.</param>
    public static void LogDebugMessage(
        this ILogger logger,
        Exception ex,
        string message,
        object[] arguments,
        [CallerMemberName] string caller = null)
    {
        RunLoggerSafely(
            () =>
            {
                message = $"{{caller}}(): Exception of type {ex.GetType()} occurred. {message}";
                logger?.LogDebug(ex, message, new object[] { caller }.Concat(arguments).ToArray());
            },
            logger,
            message,
            caller,
            arguments.Length);
    }

    /// <summary>
    ///     Uses the LogError method of <see cref="LoggerExtensions" /> with additional information
    ///     for better logging. Includes also an exception parameter.
    /// </summary>
    /// <param name="logger">The given logger.</param>
    /// <param name="message">The message which should be logged.</param>
    /// <param name="ex">Exception which should be logged and not thrown.</param>
    /// <param name="arguments">Arguments to pass to the logger.</param>
    /// <param name="caller">The method name where the logger is called.</param>
    public static void LogErrorMessage(
        this ILogger logger,
        Exception ex,
        string message,
        object[] arguments,
        [CallerMemberName] string caller = null)
    {
        RunLoggerSafely(
            () =>
            {
                if (ex == null)
                {
                    message = $"{{caller}}(): {message}";
                    logger?.LogError(message, new object[] { caller }.Concat(arguments).ToArray());
                }
                else
                {
                    message = $"{{caller}}(): Exception of type {ex.GetType()} occurred. {message}";
                    logger?.LogError(ex, message, new object[] { caller }.Concat(arguments).ToArray());
                }
            },
            logger,
            message,
            caller,
            arguments.Length);
    }

    /// <summary>
    ///     Uses the LogInformation method of <see cref="LoggerExtensions" /> with additional information
    ///     for better logging.
    /// </summary>
    /// <param name="logger">The given logger.</param>
    /// <param name="message">The message which should be logged.</param>
    /// <param name="arguments">Arguments to pass to the logger.</param>
    /// <param name="caller">The method name where the logger is called.</param>
    public static void LogInfoMessage(
        this ILogger logger,
        string message,
        object[] arguments,
        [CallerMemberName] string caller = null)
    {
        RunLoggerSafely(
            () =>
            {
                message = $"{{caller}}: {message}";
                logger?.LogInformation(message, new object[] { caller }.Concat(arguments).ToArray());
            },
            logger,
            message,
            caller,
            arguments.Length);
    }

    /// <summary>
    ///     Uses the LogTrace method of <see cref="LoggerExtensions" /> with additional information
    ///     for better logging.
    /// </summary>
    /// <param name="logger">The given logger.</param>
    /// <param name="message">The message which should be logged.</param>
    /// <param name="arguments">Arguments to pass to the logger.</param>
    /// <param name="caller">The method name where the logger is called.</param>
    public static void LogTraceMessage(
        this ILogger logger,
        string message,
        object[] arguments,
        [CallerMemberName] string caller = null)
    {
        RunLoggerSafely(
            () =>
            {
                message = $"{{caller}}(): {message}";
                logger?.LogTrace(message, new object[] { caller }.Concat(arguments).ToArray());
            },
            logger,
            message,
            caller,
            arguments.Length);
    }

    /// <summary>
    ///     Uses the LogWarning method of <see cref="LoggerExtensions" /> with additional information
    ///     for better logging.
    /// </summary>
    /// <param name="logger">The given logger.</param>
    /// <param name="message">The message which should be logged.</param>
    /// <param name="arguments">Arguments to pass to the logger.</param>
    /// <param name="caller">The method name where the logger is called.</param>
    public static void LogWarnMessage(
        this ILogger logger,
        string message,
        object[] arguments,
        [CallerMemberName] string caller = null)
    {
        RunLoggerSafely(
            () =>
            {
                message = $"{{caller}}(): {message}";
                logger?.LogWarning(message, new object[] { caller }.Concat(arguments).ToArray());
            },
            logger,
            message,
            caller,
            arguments.Length);
    }

    /// <summary>
    ///     Uses the LogWarning method of <see cref="LoggerExtensions" /> with additional information
    ///     for better logging. Includes also an exception parameter.
    /// </summary>
    /// <param name="logger">The given logger.</param>
    /// <param name="message">The message which should be logged.</param>
    /// <param name="arguments">Arguments to pass to the logger.</param>
    /// <param name="ex">Exception which should be logged and not thrown.</param>
    /// <param name="caller">The method name where the logger is called.</param>
    public static void LogWarnMessage(
        this ILogger logger,
        string message,
        object[] arguments,
        Exception ex,
        [CallerMemberName] string caller = null)
    {
        RunLoggerSafely(
            () =>
            {
                if (ex == null)
                {
                    message = $"{{caller}}(): {message}";
                    logger?.LogWarning(message, new object[] { caller }.Concat(arguments).ToArray());
                }
                else
                {
                    message =
                        $"{{caller}}(): Exception of type {ex.GetType()} occurred. {message}. Exception message: {ex.Message}.";

                    logger?.LogWarning(ex, message, new object[] { caller }.Concat(arguments).ToArray());
                }
            },
            logger,
            message,
            caller,
            arguments.Length);
    }

    /// <summary>
    ///     Uses the LogWarning method of <see cref="LoggerExtensions" /> with additional information
    ///     for better logging. Includes also an exception parameter.
    /// </summary>
    /// <param name="logger">The given logger.</param>
    /// <param name="message">The message which should be logged.</param>
    /// <param name="ex">Exception which should be logged and not thrown.</param>
    /// <param name="arguments">Arguments to pass to the logger.</param>
    /// <param name="caller">The method name where the logger is called.</param>
    public static void LogWarnMessage(
        this ILogger logger,
        Exception ex,
        string message,
        object[] arguments,
        [CallerMemberName] string caller = null)
    {
        RunLoggerSafely(
            () =>
            {
                if (ex == null)
                {
                    message = $"{{caller}}(): {message}";
                    logger?.LogWarning(message, new object[] { caller }.Concat(arguments).ToArray());
                }
                else
                {
                    message = $"{{caller}}():  Exception of type {ex.GetType()} occurred. {message}";
                    logger?.LogWarning(ex, message, new object[] { caller }.Concat(arguments).ToArray());
                }
            },
            logger,
            message,
            caller,
            arguments.Length);
    }

    /// <summary>
    ///     Checks if the given logger instance is not null and if log level trace is enabled for this instance.
    /// </summary>
    /// <param name="logger">The logger to be used for the check.</param>
    /// <returns>True if logging level trace is enabled.</returns>
    public static bool IsEnabledForTrace(
        this ILogger logger)
    {
        return IsEnabledFor(logger, LogLevel.Trace);
    }

    /// <summary>
    ///     Checks if the given logger instance is not null and if log level trace is enabled for this instance.
    /// </summary>
    /// <param name="logger">The logger to be used for the check.</param>
    /// <returns>True if logging level debug is enabled.</returns>
    public static bool IsEnabledForDebug(
        this ILogger logger)
    {
        return IsEnabledFor(logger, LogLevel.Debug);
    }

    /// <summary>
    ///     Checks if the given logger instance is not null and if the given log level is enabled for this instance.
    /// </summary>
    /// <param name="logger">The logger to be used for the check.</param>
    /// <param name="logLevel">Level to be checked.</param>
    /// <returns>True if logging is enabled.</returns>
    public static bool IsEnabledFor(
        this ILogger logger,
        LogLevel logLevel)
    {
        return logger != null && logger.IsEnabled(logLevel);
    }

    private class MethodScopeToken : IDisposable
    {
        private readonly string _Caller;
        private readonly ILogger _Logger;

        public MethodScopeToken(ILogger logger, string caller)
        {
            _Logger = logger;
            _Caller = caller;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _Logger.ExitMethod(_Caller);
        }
    }
}
