using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AutoMapper;
using Maverick.UserProfileService.AggregateEvents.Common;
using Maverick.UserProfileService.Models.Models;
using Microsoft.Extensions.Logging;
using UserProfileService.Common;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.EventSourcing.Abstractions;
using UserProfileService.EventSourcing.Abstractions.Models;
using UserProfileService.Projection.FirstLevel.Abstractions;
using UserProfileService.Projection.FirstLevel.Extensions;
using EventInitiator = Maverick.UserProfileService.AggregateEvents.Common.EventInitiator;

namespace UserProfileService.Projection.FirstLevel.Implementation;

/// <summary>
///     The first level event creator transforms the normal <see cref="IUserProfileServiceEvent" />
///     to a <see cref="EventTuple" />.
/// </summary>
internal class FirstLevelEventCreator : IFirstLevelEventTupleCreator
{
    private readonly ILogger<FirstLevelEventCreator> _Logger;
    private readonly IMapper _Mapper;
    private readonly IStreamNameResolver _StreamNameResolver;

    /// <summary>
    ///     Creates an instance of the object <see cref="FirstLevelEventCreator" />.
    /// </summary>
    /// <param name="mapper">The mapper is used for mapping object to another object.</param>
    /// <param name="streamNameResolver">The stream name resolver is used to resolve the stream name.</param>
    /// <param name="logger">The logger is used to log messages with various level.</param>
    public FirstLevelEventCreator(
        IMapper mapper,
        IStreamNameResolver streamNameResolver,
        ILogger<FirstLevelEventCreator> logger)
    {
        _Mapper = mapper;
        _StreamNameResolver = streamNameResolver;
        _Logger = logger;
    }

    /// <summary>
    ///     The method is used to add the metadata for the <inheritdoc cref="IUserProfileServiceEvent" />
    ///     event.
    /// </summary>
    /// <param name="upsEvent">The usp event is used to add the metadata to the event.</param>
    /// <param name="domainEvent">
    ///     The initial event from which the metadata will be extracted. If not provided, default values
    ///     will taken.
    /// </param>
    /// <param name="relatedEntityId">Alternative id of the entity that all events are based on.</param>
    /// <returns>The <see cref="IUserProfileServiceEvent" /> which metadata are filled. </returns>
    /// <exception cref="ArgumentNullException"><paramref name="upsEvent" /> is null.</exception>
    private IUserProfileServiceEvent AddMetaData(
        IUserProfileServiceEvent upsEvent,
        string relatedEntityId,
        IDomainEvent domainEvent = null)
    {
        _Logger.EnterMethod();

        if (upsEvent == null)
        {
            throw new ArgumentNullException(nameof(upsEvent));
        }

        string correlationId = domainEvent.GetCorrelationId(Activity.Current);

        if (string.IsNullOrEmpty(correlationId))
        {
            correlationId = Activity.Current?.Id;
            //TODO: should be activated
            //throw new ArgumentNullException(nameof(correlationId));
        }

        string relEntityId = domainEvent.GetRelatedEntityId(relatedEntityId);

        EventInitiator eventInitiator = domainEvent.GetEventInitiator(e => _Mapper.Map<EventInitiator>(e));

        //In the future check if the initiator is always set.
        //First, the initiator is set to unknown to avoid errors.
        eventInitiator ??= new EventInitiator();

        upsEvent.EventId = Guid.NewGuid().ToString();
        upsEvent.MetaData.ProcessId = domainEvent.GetProcessId();
        upsEvent.MetaData.CorrelationId = correlationId;
        upsEvent.MetaData.Initiator = eventInitiator;
        upsEvent.MetaData.VersionInformation = 1;
        upsEvent.MetaData.RelatedEntityId = relEntityId;
        upsEvent.MetaData.Timestamp = domainEvent?.Timestamp ?? DateTime.MinValue.ToUniversalTime();

        _Logger.LogInfoMessage(
            "The eventId:{evenId}, ProcessId: {processId}, CorrelationId: {correlationId}, eventInitiator:{eventInitiator}, versionInformation:{versionInformation}",
            LogHelpers.Arguments(
                upsEvent.EventId,
                upsEvent.MetaData.ProcessId,
                upsEvent.MetaData.CorrelationId,
                upsEvent.MetaData.Initiator.ToLogString(),
                upsEvent.MetaData.VersionInformation.ToLogString()));

        return _Logger.ExitMethod(upsEvent);
    }

    ///<inheritdoc />
    public EventTuple CreateEvent(
        ObjectIdent resourceId,
        IUserProfileServiceEvent upsEvent,
        IDomainEvent domainEvent = null)
    {
        _Logger.EnterMethod();

        if (resourceId == null)
        {
            throw new ArgumentNullException(nameof(resourceId));
        }

        if (upsEvent == null)
        {
            throw new ArgumentNullException(nameof(upsEvent));
        }

        var eventTuple = new EventTuple(_StreamNameResolver.GetStreamName(resourceId), upsEvent);
        AddMetaData(upsEvent, _StreamNameResolver.GetStreamName(resourceId), domainEvent);

        return _Logger.ExitMethod(eventTuple);
    }

    ///<inheritdoc />
    public IEnumerable<EventTuple> CreateEvents(
        ObjectIdent resourceId,
        IEnumerable<IUserProfileServiceEvent> uspEvents,
        IDomainEvent domainEvent = null)
    {
        if (resourceId == null)
        {
            throw new ArgumentNullException(nameof(resourceId));
        }

        if (uspEvents == null)
        {
            throw new ArgumentNullException(nameof(uspEvents));
        }

        return _Logger.ExitMethod(
            uspEvents.Select(
                    eve => CreateEvent(
                        resourceId,
                        eve,
                        domainEvent))
                .ToList());
    }
}
