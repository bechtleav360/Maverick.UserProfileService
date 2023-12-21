using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace UserProfileService.Common.V2.Utilities;

/// <summary>
///     Provides helper methods for executing asynchronous operations safely and handling specific exceptions.
/// </summary>
/// <typeparam name="TException">The type of exception to handle.</typeparam>
public static class ExecutionHelpers<TException> where TException : Exception
{
    /// <summary>
    ///     Executes an asynchronous operation safely and handles the specified exception.
    /// </summary>
    /// <param name="executionMethod">The asynchronous method to execute.</param>
    /// <param name="logger">The logger to use for logging.</param>
    /// <param name="exceptionHandling">
    ///     An action to handle the exception with the specified logger.
    ///     It receives the logger and the caught exception as parameters.
    /// </param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task RunSafelyAsync(
        Func<CancellationToken, Task> executionMethod,
        ILogger logger = null,
        Action<ILogger, TException> exceptionHandling = null,
        CancellationToken cancellationToken = default)
    {
        if (executionMethod == null)
        {
            throw new ArgumentNullException(nameof(executionMethod));
        }

        try
        {
            await executionMethod.Invoke(cancellationToken);
        }
        catch (TException e)
        {
            exceptionHandling.Invoke(logger, e);
        }
    }
}
