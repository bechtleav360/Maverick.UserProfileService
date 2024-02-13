using System;
using System.Threading;
using System.Threading.Tasks;
using Maverick.UserProfileService.AggregateEvents.Common;
using Microsoft.Extensions.Logging;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.EventSourcing.Abstractions.Models;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Common.Extensions;

namespace UserProfileService.Projection.FirstLevel.Abstractions;

/// <summary>
///     The base class for eventObject handlers.
/// </summary>
/// <typeparam name="TEventType">
///     The type of eventObject that is handled.
/// </typeparam>
public abstract class FirstLevelEventHandlerBase<TEventType> : IFirstLevelProjectionEventHandler<TEventType>
    where TEventType : class, IUserProfileServiceEvent
{
    /// <summary>
    ///     The <see cref="ILogger" /> to be used.
    /// </summary>
    protected readonly ILogger Logger;
    /// <summary>
    ///    he repository to be used to persist first level projection data. 
    /// </summary>
    protected readonly IFirstLevelProjectionRepository Repository;

    /// <summary>
    ///     Creates a new instance of <see cref="FirstLevelEventHandlerBase{TEventType}" />
    /// </summary>
    /// <param name="logger">The <see cref="ILogger" /> to be used.</param>
    /// <param name="repository">The repository to be used to persist first level projection data.</param>
    protected FirstLevelEventHandlerBase(ILogger logger, IFirstLevelProjectionRepository repository)
    {
        Logger = logger;
        Repository = repository;
    }

    /// <summary>
    ///     Executes a <paramref name="methodDefinition" /> inside a transaction.
    /// </summary>
    /// <param name="methodDefinition">The method to be executed inside a transaction.</param>
    /// <param name="streamEvent"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>A task that represent the asynchronous write operation.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="methodDefinition" /> is <c>null</c></exception>
    protected virtual async Task ExecuteInsideTransactionAsync(
        Func<IFirstLevelProjectionRepository, IDatabaseTransaction, CancellationToken, Task> methodDefinition,
        StreamedEventHeader streamEvent,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (methodDefinition == null)
        {
            throw new ArgumentNullException(nameof(methodDefinition));
        }

        DateTimeOffset start = DateTimeOffset.UtcNow;
        IDatabaseTransaction transaction = await Repository.StartTransactionAsync(cancellationToken);

        try
        {
            await methodDefinition.Invoke(Repository, transaction, cancellationToken);

            if (!await Repository.TrySaveProjectionStateAsync(
                    streamEvent.ToProjectionState(processingStarted: start),
                    transaction,
                    Logger,
                    cancellationToken))
            {
                Logger.LogInfoMessage("Could not save state.", LogHelpers.Arguments());
            }

            await Repository.CommitTransactionAsync(transaction, cancellationToken);
            Logger.ExitMethod();
        }
        catch (Exception e)
        {
            try
            {
                if (!await Repository.TrySaveProjectionStateAsync(
                        streamEvent.ToProjectionState(e, processingStarted: start),
                        // in this case the transaction maybe not valid any more
                        null,
                        Logger,
                        cancellationToken))
                {
                    Logger.LogInfoMessage("Could not save state (already in error)", LogHelpers.Arguments());
                }

                await Repository.AbortTransactionAsync(transaction, cancellationToken);
            }
            catch (Exception exception)
            {
                // the abort can be fail because the transaction can be deleted before commitment.
                if (Logger.IsEnabledForTrace())
                {
                    Logger.LogTraceMessage(
                        "Transaction could not been aborted. Error message: {errorMessage}; Transaction: {transaction}",
                        LogHelpers.Arguments(exception.Message, transaction.ToLogString()));
                }
            }

            throw;
        }
    }
    
    /// <summary>
    ///     Handles the eventObject with eventObject specific processes.
    /// </summary>
    /// <param name="eventObject">The eventObject object that occurred.</param>
    /// <param name="streamEvent"></param>
    /// <param name="transaction"> Defines an object as a result of a started transaction.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>A task that represent the asynchronous  operation.</returns>
    protected abstract Task HandleInternalAsync(
        TEventType eventObject,
        StreamedEventHeader streamEvent,
        IDatabaseTransaction transaction,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     This method is used to run a method in a safe manner.
    ///     It has execution method and a compensate method that should be
    ///     executed, when the other method fails. Exception throwing can be
    ///     switch on/off.
    /// </summary>
    /// <param name="methodToExecute">The method that should be executed in a safe manner.</param>
    /// <param name="methodToCompensate">The method that should be executed, when the first method fails.</param>
    /// <param name="throwException">Switch exception on/off.</param>
    /// <returns>A task that represent the asynchronous  operation.</returns>
    /// <exception cref="ArgumentNullException">Is thrown when one method is null.</exception>
    protected async Task ExecuteSafelyAsync(
        Func<Task> methodToExecute,
        Func<Task> methodToCompensate,
        bool throwException = true)
    {
        if (methodToExecute == null)
        {
            throw new ArgumentNullException(nameof(methodToExecute));
        }

        if (methodToCompensate == null)
        {
            throw new ArgumentNullException(nameof(methodToCompensate));
        }

        try
        {
            await methodToExecute();
        }
        catch (Exception)
        {
            await methodToCompensate();

            if (throwException)
            {
                throw;
            }
        }
    }

    /// <summary>
    ///     This method is used to run a method in a safe manner.
    ///     It has execution method and a compensate method that should be
    ///     executed, when the other method fails. Exception throwing can be
    ///     switch on/off.
    ///     The default value of the result item can be set.
    /// </summary>
    /// <typeparam name="TResult">The type of the resulting object of <paramref name="methodToExecute" /> and this method.</typeparam>
    /// <param name="methodToExecute">The method that should be executed in a safe manner.</param>
    /// <param name="defaultValue">If no exception should be thrown, but one has been caught, this value will be returned.</param>
    /// <param name="methodToCompensate">The method that should be executed, when the first method fails (optional).</param>
    /// <param name="throwException">Switch exception on/off.</param>
    /// <returns>A task that represent the asynchronous operation. It wraps the result item.</returns>
    /// <exception cref="ArgumentNullException">Is thrown when one method is null.</exception>
    protected async Task<TResult> ExecuteSafelyAsync<TResult>(
        Func<Task<TResult>> methodToExecute,
        TResult defaultValue = default,
        Func<Task> methodToCompensate = null,
        bool throwException = false)
    {
        if (methodToExecute == null)
        {
            throw new ArgumentNullException(nameof(methodToExecute));
        }

        try
        {
            return await methodToExecute();
        }
        catch (Exception)
        {
            if (methodToCompensate != null)
            {
                await methodToCompensate();
            }

            if (throwException)
            {
                throw;
            }

            return defaultValue;
        }
    }

    /// <inheritdoc />
    public async Task HandleEventAsync(
        TEventType eventObject,
        StreamedEventHeader streamEvent,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (eventObject == null)
        {
            Logger.LogWarnMessage(
                "Provided domain eventObject is null. This handler {handlerType}<{eventType}> cannot proceed.",
                LogHelpers.Arguments(GetType().Name, typeof(TEventType).Name));

            throw new ArgumentNullException(nameof(eventObject), "Domain eventObject must not be null.");
        }

        if (streamEvent == null)
        {
            throw new ArgumentNullException(nameof(streamEvent));
        }

        if (Logger.IsEnabledForTrace())
        {
            Logger.LogTraceMessage(
                "Received eventObject:\n {{eventString}}, \n with streamEvent: {streamEvent} for checkpoint\n.",
                LogHelpers.Arguments(eventObject.ToLogString(), streamEvent.ToLogString()));
        }

        Logger.LogInfoMessage(
            "Received eventObject of type {eventName}.",
            LogHelpers.Arguments(typeof(TEventType).Name));

        // Each eventObject handler handles its corresponding eventObject.
        // Projection it is saved after completion of the handler. 
        await ExecuteInsideTransactionAsync(
            (repo, transaction, cT) =>
                HandleInternalAsync((TEventType)eventObject, streamEvent, transaction, cT),
            streamEvent,
            cancellationToken);

        Logger.LogInfoMessage(
            "Event of type {eventName} handled successfully.",
            LogHelpers.Arguments(typeof(TEventType).Name));

        Logger.ExitMethod();
    }

    /// <inheritdoc />
    public virtual Task HandleEventAsync(
        IUserProfileServiceEvent domainEvent,
        StreamedEventHeader eventHeader,
        CancellationToken cancellationToken = default)
    {
        return HandleEventAsync((TEventType)domainEvent, eventHeader, cancellationToken);
    }
}