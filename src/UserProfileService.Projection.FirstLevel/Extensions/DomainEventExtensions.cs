using System;
using System.Diagnostics;
using UserProfileService.EventSourcing.Abstractions.Models;
using EventInitiator = Maverick.UserProfileService.AggregateEvents.Common.EventInitiator;

namespace UserProfileService.Projection.FirstLevel.Extensions;

internal static class DomainEventExtensions
{
    internal static string GetRelatedEntityId(this IDomainEvent domainEvent, string relatedEntityId = null)
    {
        return domainEvent?.MetaData?.RelatedEntityId ?? relatedEntityId;
    }

    internal static string GetProcessId(this IDomainEvent domainEvent)
    {
        return domainEvent?.RequestSagaId ?? domainEvent?.MetaData?.ProcessId;
    }

    internal static string GetCorrelationId(this IDomainEvent domainEvent, Activity activity = null)
    {
        return domainEvent?.CorrelationId ?? domainEvent?.MetaData?.CorrelationId ?? activity?.Id;
    }

    internal static EventInitiator GetEventInitiator(
        this IDomainEvent domainEvent,
        Func<EventSourcing.Abstractions.Models.EventInitiator, EventInitiator> initiatorConverter)
    {
        if (domainEvent == null)
        {
            return EventInitiator.SystemInitiator;
        }

        if (domainEvent.Initiator != null)
        {
            return initiatorConverter.Invoke(domainEvent.Initiator);
        }

        return domainEvent.MetaData?.Initiator;
    }
}
