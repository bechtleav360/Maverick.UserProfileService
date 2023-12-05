using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Maverick.UserProfileService.AggregateEvents.Common;
using Maverick.UserProfileService.AggregateEvents.V1;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.EventSourcing.Abstractions;
using UserProfileService.EventSourcing.Abstractions.Exceptions;
using UserProfileService.EventSourcing.Abstractions.Models;
using UserProfileService.Informer.Abstraction;
using UserProfileService.Informer.Implementations;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Common.Extensions;

namespace UserProfileService.Projection.SecondLevel.Abstractions;

/// <summary>
///     Represents as base class for second-level event handler.
/// </summary>
public abstract class SecondLevelEventHandlerBase<TEvent> : ISecondLevelEventHandler<TEvent>
    where TEvent : class, IUserProfileServiceEvent
{
    
    /// <summary>
    ///     The logger to be used by the current instance of <see cref="SecondLevelEventHandlerBase{TEvent}" />.
    ///     implementation.
    /// </summary>
    protected ILogger Logger { get; }

    /// <summary>
    ///     The repository to be used to persist the event data in a specified way.
    /// </summary>
    protected ISecondLevelProjectionRepository Repository { get; }
    
    /// <summary>
    ///     Mapper as helper for converting model classes.
    /// </summary>
    protected IMapper Mapper { get; }

    /// <summary>
    ///     Resolver that resolve an <see cref="ObjectIdent" /> to a stream name and vice versa.
    /// </summary>
    protected IStreamNameResolver StreamNameResolver { get; }
    
    /// <summary>
    ///  The message informer is used to notify consumer (if present) when
    ///  an event message type appears.
    /// </summary>
    protected IMessageInformer MessageInformer { get; }

    /// <summary>
    ///     The default constructor of the <see cref="SecondLevelEventHandlerBase{TEvent}" /> class that will accept a
    ///     specified
    ///     <see cref="ILogger" /> instance.
    /// </summary>
    /// <param name="repository">The repository to be used.</param>
    /// <param name="mapper">Mapper to be used to convert event instances.</param>
    /// <param name="streamNameResolver">The resolver that will convert from or to stream names.</param>
    /// <param name="messageInformer"> The message informer is used to notify consumer (if present) when
    ///  an event message type appears.</param>
    /// <param name="logger">The <see cref="ILogger" /> to be used.</param>
    protected SecondLevelEventHandlerBase(
        ISecondLevelProjectionRepository repository,
        IMapper mapper,
        IStreamNameResolver streamNameResolver,
        IMessageInformer messageInformer,
        ILogger logger)
    {
        Repository = repository;
        Mapper = mapper;
        StreamNameResolver = streamNameResolver;
        Logger = logger;
        MessageInformer = messageInformer;
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
        Func<ISecondLevelProjectionRepository, IDatabaseTransaction, CancellationToken, Task> methodDefinition,
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
                Logger.LogInfoMessage("Could not save state!", LogHelpers.Arguments());
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

        if (relatedObjectIdent == null
            || string.IsNullOrWhiteSpace(relatedObjectIdent.Id)
            || relatedObjectIdent.Type == ObjectType.Unknown)
        {
            throw new InvalidHeaderException(
                "The EventStreamId is malformed. Could not extract the correct object identifier."
                + "Could not get a valid object identifier from stream name. Skipping method",
                eventHeader);
        }

        Logger.LogDebugMessage(
            "Trying to get additional data for a notification message. The event type is {eventType}",
            LogHelpers.Arguments(typeof(TEvent)));
        
        try
        {
            INotifyContext contextData = await GetAdditionalMessageContextAsync(domainEvent, relatedObjectIdent);

            Logger.LogDebugMessage(
                "The additional context information: {additionalInformation}.",
                LogHelpers.Arguments(contextData.ContextType.ToLogString()));
            
            await HandleEventAsync(domainEvent, eventHeader, relatedObjectIdent, cancellationToken);

            Logger.LogDebug(
                "Tying to send a notification to a consumer. The event is of type {eventType}.",
                LogHelpers.Arguments(typeof(TEvent)));
            

            await MessageInformer.NotifyEventOccurredAsync(domainEvent, contextData);
            
        }
        catch (Exception ex)
        {
            Logger.LogErrorMessage(ex, ex.Message, LogHelpers.Arguments());

            throw;
        }
        Logger.ExitMethod();
    }

    /// <summary>
    ///     Is used for additional information  that are needed to create a notification
    ///     for a special consumer. If additional information for a special event are needed
    ///     this method can be overwritten and the information can be added.
    /// </summary>
    /// <param name="serviceEvent">The event for which the additional data are needed.</param>
    /// <param name="relatedObject">Get the related object information.</param>
    /// <returns>
    ///     A task representing the asynchronous operation that wraps an <see cref="INotifyContext" />
    ///     that contains the additional data for a notification.
    /// </returns>
    protected virtual async Task<INotifyContext> GetAdditionalMessageContextAsync(
        IUserProfileServiceEvent serviceEvent,
        ObjectIdent relatedObject)
    {
        var defaultNotifyContext = new DefaultNotifyContext();

        if (serviceEvent is EntityDeleted entityDeleted && relatedObject.Type == ObjectType.User)
        {
            ISecondLevelProjectionProfile profile = await Repository.GetProfileAsync(entityDeleted.Id);
            defaultNotifyContext.ExternalIdentifier = profile?.ExternalIds.FirstOrDefault().Id;
            defaultNotifyContext.ContextType = relatedObject;
        }

        return defaultNotifyContext;
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
