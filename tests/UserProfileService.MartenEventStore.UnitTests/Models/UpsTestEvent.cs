using Maverick.UserProfileService.AggregateEvents.Common;

namespace UserProfileService.MartenEventStore.UnitTests.Models;

public class UpsTestEvent : IUserProfileServiceEvent
{
    public string? EventId { get; set; }
    public string? EventName { get; set; }
    public EventMetaData? MetaData { get; set; }
    public string? Type { get; set; }
}
