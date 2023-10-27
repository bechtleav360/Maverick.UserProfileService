using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models;

namespace UserProfileService.Projection.FirstLevel.Tests.Extensions
{
    public static class PropertiesChangedExtension
    {
        public static PropertiesChanged SetRelatedContext(
            this PropertiesChanged propertiesChangedEvent,
            PropertiesChangedContext context)
        {
            propertiesChangedEvent.RelatedContext = context;

            return propertiesChangedEvent;
        }
    }
}
