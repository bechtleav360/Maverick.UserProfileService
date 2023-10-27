using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Maverick.UserProfileService.Models.Models;
using Microsoft.Extensions.Logging;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Projection.Abstractions;

namespace UserProfileService.Projection.Common.Extensions;

/// <summary>
///     Contains extensions methods regarding a <see cref="IProjectionStateRepository" />.
/// </summary>
public static class ProjectionStateRepositoryExtensions
{
    private static async Task<bool> TrySaveProjectionStateInternallyAsync<TRepository>(
        TRepository stateRepository,
        ProjectionState projectionState,
        bool logExceptionAsError,
        IDatabaseTransaction transaction = null,
        ILogger logger = null,
        CancellationToken cancellationToken = default)
        where TRepository : class, IProjectionStateRepository
    {
        try
        {
            await stateRepository.SaveProjectionStateAsync(
                projectionState,
                transaction,
                cancellationToken);

            return true;
        }
        // cancelled request should not logged as error at all
        catch (OperationCanceledException)
        {
            logger.LogDebugMessage("Task has been cancelled.", LogHelpers.Arguments());

            return false;
        }
        catch (Exception e)
        {
            if (logExceptionAsError)
            {
                logger.LogErrorMessage(
                    e,
                    "Error occurred during saving projection state. {errorMessage}"
                        .AppendProjectionStateContextTemplate(),
                    LogHelpers.Arguments(e.Message)
                        .AppendProjectionStateContext(projectionState));

                return false;
            }

            logger.LogDebugMessage(
                e,
                "Error occurred during saving projection state. {errorMessage}"
                    .AppendProjectionStateContextTemplate(),
                LogHelpers.Arguments(e.Message).AppendProjectionStateContext(projectionState));

            return false;
        }
    }

    private static void LogWithContext(
        this ILogger logger,
        LogLevel logLevel,
        ProjectionState context,
        string message,
        [CallerMemberName] string caller = null)
    {
        if (logger == null
            || !logger.IsEnabledFor(logLevel))
        {
            return;
        }

        string templateText = message.AppendProjectionStateContextTemplate();
        object[] arguments = LogHelpers.Arguments().AppendProjectionStateContext(context);

        switch (logLevel)
        {
            case LogLevel.Trace:
                logger.LogTraceMessage(templateText, arguments, caller);

                break;
            case LogLevel.Debug:
                logger.LogDebugMessage(templateText, arguments, caller);

                break;
            case LogLevel.Information:
                logger.LogInfoMessage(templateText, arguments, caller);

                break;
            case LogLevel.Warning:
                logger.LogWarnMessage(templateText, arguments, caller);

                break;
            case LogLevel.Error:
                logger.LogErrorMessage(null, templateText, arguments, caller);

                break;
            case LogLevel.Critical:
                logger.LogErrorMessage(null, templateText, arguments, caller);

                break;
            case LogLevel.None:
            default:
                return;
        }
    }

    private static object[] AppendProjectionStateContext(
        this object[] currentArguments,
        ProjectionState projectionState)
    {
        return currentArguments.Concat(
                new object[]
                {
                    projectionState.EventId,
                    projectionState.EventName,
                    projectionState.EventNumberVersion,
                    projectionState.StreamName
                })
            .ToArray();
    }

    private static string AppendProjectionStateContextTemplate(
        this string currentTemplate)
    {
        return
            $"{currentTemplate} (event id: {{eventId}}; event name: {{eventName}}; event number: {{eventNumber}}; event stream: {{eventStream}})";
    }

    /// <summary>
    ///     Tries to save projection state and returns a boolean value indicating the success of the operation. An optional
    ///     <see cref="ILogger" /> can be used to log error or warnings.
    /// </summary>
    /// <typeparam name="TRepository">
    ///     The type of <see cref="IProjectionStateRepository" /> that will take care of the write
    ///     requests.
    /// </typeparam>
    /// <param name="stateRepository">The state repository to be used to store the <paramref name="projectionState" />.</param>
    /// <param name="projectionState">The state to be saved.</param>
    /// <param name="transaction">
    ///     An optional parameter containing information about the current transaction. This won't be
    ///     used for a second try of the write operation.
    /// </param>
    /// <param name="logger">The logger to be used.</param>
    /// <param name="cancellationToken">The token to monitor cancellation requests.</param>
    /// <returns>
    ///     A task representing the asynchronous write operation. It wraps a boolean value that will be <c>true</c>, if
    ///     the operation has been successful, otherwise <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static async Task<bool> TrySaveProjectionStateAsync<TRepository>(
        this TRepository stateRepository,
        ProjectionState projectionState,
        IDatabaseTransaction transaction = null,
        ILogger logger = null,
        CancellationToken cancellationToken = default)
        where TRepository : class, IProjectionStateRepository
    {
        logger.EnterMethod();

        if (stateRepository == null)
        {
            throw new ArgumentNullException(nameof(stateRepository));
        }

        if (projectionState == null)
        {
            logger.LogWarnMessage(
                "Projection state could not be saved, because it is null/empty.",
                LogHelpers.Arguments());

            return logger.ExitMethod(false);
        }

        if (logger.IsEnabledForTrace())
        {
            logger.LogTraceMessage(
                "Input parameter: projectionState: {projectionState}",
                projectionState.ToLogString().AsArgumentList());
        }

        bool firstTrySuccess = await TrySaveProjectionStateInternallyAsync(
            stateRepository,
            projectionState,
            false,
            transaction,
            logger,
            cancellationToken);

        if (firstTrySuccess)
        {
            logger.LogWithContext(
                LogLevel.Debug,
                projectionState,
                "New projection state has been saved");

            return logger.ExitMethod(true);
        }

        if (cancellationToken.IsCancellationRequested)
        {
            logger.LogDebugMessage("Task has been cancelled.", LogHelpers.Arguments());

            return logger.ExitMethod(false);
        }

        logger.LogWithContext(
            LogLevel.Information,
            projectionState,
            "Could not save projection state. Will try one more time");

        bool secondTrySuccess = await TrySaveProjectionStateInternallyAsync(
            stateRepository,
            projectionState,
            true,
            // because of the second try, no transaction will be used for the second approach (maybe it is not valid anymore)
            null,
            logger,
            cancellationToken);

        if (secondTrySuccess)
        {
            logger.LogWithContext(
                LogLevel.Information,
                projectionState,
                "New projection state has been saved as second try");

            return logger.ExitMethod(true);
        }

        return logger.ExitMethod(false);
    }
}
