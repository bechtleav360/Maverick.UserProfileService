using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Maverick.UserProfileService.AggregateEvents.Common;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using Microsoft.Extensions.Logging;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.EventSourcing.Abstractions;
using UserProfileService.EventSourcing.Abstractions.Exceptions;
using UserProfileService.EventSourcing.Abstractions.Models;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Common.Extensions;

namespace UserProfileService.Projection.SecondLevel.Assignments.Abstractions;

/// <summary>
///     Represents as base class for second-level event handler.
/// </summary>
public abstract class SecondLevelAssignmentEventHandlerBase<TEvent> : ISecondLevelAssignmentEventHandler<TEvent>
    where TEvent : class, IUserProfileServiceEvent
{
    /// <summary>
    ///     The logger to be used by the current instance of <see cref="SecondLevelAssignmentEventHandlerBase{TEvent}" />.
    ///     implementation.
    /// </summary>
    protected ILogger Logger { get; }

    /// <summary>
    ///     The repository to be used to persist the event data in a specified way.
    /// </summary>
    protected ISecondLevelAssignmentRepository Repository { get; }

    /// <summary>
    ///     Mapper as helper for converting model classes.
    /// </summary>
    public IMapper Mapper { get; }

    public IStreamNameResolver StreamNameResolver { get; }

    /// <summary>
    ///     The default constructor of the <see cref="SecondLevelAssignmentEventHandlerBase{TEvent}" /> class that will accept
    ///     a
    ///     specified
    ///     <see cref="ILogger" /> instance.
    /// </summary>
    /// <param name="repository">The repository to be used.</param>
    /// <param name="mapper">Mapper to be used to convert event instances.</param>
    /// <param name="streamNameResolver">The resolver that will convert from or to stream names.</param>
    /// <param name="logger">The <see cref="ILogger" /> to be used.</param>
    protected SecondLevelAssignmentEventHandlerBase(
        ISecondLevelAssignmentRepository repository,
        IMapper mapper,
        IStreamNameResolver streamNameResolver,
        ILogger logger)
    {
        Repository = repository;
        Mapper = mapper;
        StreamNameResolver = streamNameResolver;
        Logger = logger;
    }

    /// <summary>
    ///     Take care of a provided <paramref name="domainEvent" />, if necessary. This method will be defined for each event.
    /// </summary>
    /// <param name="domainEvent">The domain to be handled.</param>
    /// <param name="relatedEntityIdent">
    ///     The object identifier of the entity that is related to <paramref name="domainEvent" />
    /// </param>
    /// <param name="cancellationToken">The token to monitor and propagate cancellation requests.</param>
    /// <param name="eventHeader">Containing further information about the <paramref name="domainEvent" />.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected abstract Task HandleEventAsync(
        TEvent domainEvent,
        StreamedEventHeader eventHeader,
        ObjectIdent relatedEntityIdent,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Executes a <paramref name="methodDefinition" /> inside a transaction.
    /// </summary>
    /// <param name="methodDefinition">The method to be executed inside a transaction.</param>
    /// <param name="eventHeader">Containing further information about the event to be processed by this method.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">The <paramref name="methodDefinition" /> is <c>null</c></exception>
    protected virtual async Task ExecuteInsideTransactionAsync(
        Func<ISecondLevelAssignmentRepository, IDatabaseTransaction, CancellationToken, Task> methodDefinition,
        StreamedEventHeader eventHeader,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (methodDefinition == null)
        {
            throw new ArgumentNullException(nameof(methodDefinition));
        }

        DateTimeOffset startTime = DateTimeOffset.UtcNow;
        IDatabaseTransaction transaction = await Repository.StartTransactionAsync(cancellationToken);

        try
        {
            await methodDefinition.Invoke(Repository, transaction, cancellationToken);

            if (!await Repository.TrySaveProjectionStateAsync(
                    eventHeader.ToProjectionState(processingStarted: startTime),
                    transaction,
                    Logger,
                    cancellationToken))
            {
                Logger.LogInfoMessage("Could not save state", LogHelpers.Arguments());
            }

            await Repository.CommitTransactionAsync(transaction, cancellationToken);

            Logger.ExitMethod();
        }
        catch (Exception e)
        {
            try
            {
                if (!await Repository.TrySaveProjectionStateAsync(
                        eventHeader.ToProjectionState(e, processingStarted: startTime),
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

    /// <inheritdoc />
    /// <exception cref="InvalidHeaderException">
    ///     <paramref name="eventHeader" /> contains an event stream id that is
    ///     <c>null</c>, empty or whitespace<br />-or-<br />
    ///     <paramref name="eventHeader" /> contains an event stream id that is
    ///     malformed. The object type or/and the identifier could not be extracted.
    /// </exception>
    public async Task HandleEventAsync(
        TEvent domainEvent,
        StreamedEventHeader eventHeader,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (domainEvent == null)
        {
            throw new ArgumentNullException(nameof(domainEvent));
        }

        if (eventHeader == null)
        {
            throw new ArgumentNullException(nameof(eventHeader));
        }

        if (string.IsNullOrWhiteSpace(eventHeader.EventStreamId))
        {
            throw new InvalidHeaderException(
                "event stream id of eventHeader should not be empty or whitespace.",
                eventHeader);
        }

        Logger.LogTraceMessage(
            "Extracting object ident from event stream id {eventStreamId}.",
            eventHeader.EventStreamId.AsArgumentList());

        ObjectIdent relatedObjectIdent =
            StreamNameResolver.GetObjectIdentUsingStreamName(eventHeader.EventStreamId);

        if (string.IsNullOrWhiteSpace(relatedObjectIdent?.Id)
            || relatedObjectIdent.Type == ObjectType.Unknown)
        {
            throw new InvalidHeaderException(
                "The EventStreamId is malformed. Could not extract the correct object identifier."
                + " Could not get a valid object identifier from stream name. Skipping method",
                eventHeader);
        }

        await HandleEventAsync(domainEvent, eventHeader, relatedObjectIdent, cancellationToken);

        Logger.ExitMethod();
    }

    /// <inheritdoc/>
    public virtual Task HandleEventAsync(
        IUserProfileServiceEvent domainEvent,
        StreamedEventHeader eventHeader,
        CancellationToken cancellationToken = default)
    {
      return HandleEventAsync((TEvent)domainEvent, eventHeader, cancellationToken);
    }
}
