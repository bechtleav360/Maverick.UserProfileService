using System;
using Maverick.UserProfileService.AggregateEvents.Common;
using UserProfileService.EventSourcing.Abstractions.Models;
using EventInitiator = UserProfileService.EventSourcing.Abstractions.Models.EventInitiator;
using InitiatorType = UserProfileService.EventSourcing.Abstractions.Models.InitiatorType;

namespace UserProfileService.Common.Tests.Utilities.Extensions
{
    public static class AggregateResolveEventExtensions
    {
        public static StreamedEventHeader GenerateEventHeader(
            this IUserProfileServiceEvent domainEvent,
            long eventNumber,
            string streamId = null)
        {
            return new StreamedEventHeader
            {
                EventId = Guid.TryParse(domainEvent.EventId, out Guid parsed)
                    ? parsed
                    : Guid.Empty,
                EventNumberVersion = eventNumber,
                EventStreamId =
                    streamId ?? domainEvent.MetaData.RelatedEntityId,
                StreamId =
                    streamId ?? domainEvent.MetaData.RelatedEntityId,
                Created = domainEvent.MetaData.Timestamp,
                EventType = domainEvent.Type
            };
        }

        public static EventInitiator ConvertToEventStoreModel(
            this
                Maverick.UserProfileService.AggregateEvents.Common.EventInitiator eventInitiator)
        {
            return new EventInitiator
            {
                Id = eventInitiator.Id,
                Type = (InitiatorType)(int)eventInitiator.Type
            };
        }
    }
}
