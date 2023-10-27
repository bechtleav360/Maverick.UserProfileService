using Maverick.UserProfileService.AggregateEvents.Common;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;

namespace UserProfileService.Projection.FirstLevel.Tests.Extensions
{
    internal static class EventExtension
    {
        internal static IUserProfileServiceEvent SetRelatedEntityId(
            this IUserProfileServiceEvent @event,
            string relatedEntityId)
        {
            @event.MetaData.RelatedEntityId = relatedEntityId;

            return @event;
        }

        internal static PropertiesChanged CloneEvent(this PropertiesChanged changed)
        {
            return new PropertiesChanged
                   {
                       Id = changed.Id,
                       Properties = changed.Properties,
                       MetaData = changed.MetaData.CloneEventDate(),
                       ObjectType = changed.ObjectType
                   };
        }

        internal static EventMetaData CloneEventDate(this EventMetaData metaData)
        {
            return new EventMetaData
            {
                Batch = metaData?.Batch,
                CorrelationId = metaData?.CorrelationId,
                HasToBeInverted = metaData.HasToBeInverted,
                Initiator = metaData.Initiator,
                ProcessId = metaData?.ProcessId,
                RelatedEntityId = metaData?.RelatedEntityId,
                Timestamp = metaData.Timestamp,
                VersionInformation = metaData.VersionInformation
            };
        }
    }
}
