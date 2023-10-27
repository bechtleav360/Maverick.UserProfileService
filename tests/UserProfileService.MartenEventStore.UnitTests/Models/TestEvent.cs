using Maverick.UserProfileService.AggregateEvents.Common;

namespace UserProfileService.MartenEventStore.UnitTests.Models;

public class TestEvent : IUserProfileServiceEvent
{
    public int Alter { get; set; }
    public string? EventId { get; set; }
    public EventMetaData? MetaData { get; set; }
    public string? Name { get; set; }
    public bool Single { get; set; }
    public string? Type { get; set; }
}
