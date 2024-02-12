using Maverick.UserProfileService.AggregateEvents.Common;

namespace UserProfileService.Projection.SecondLevel.Assignments.UnitTests.Mocks
{
    public class MockUpsEvent : IUserProfileServiceEvent
    {
        /// <inheritdoc />
        public string Type { get; set; } = nameof(MockUpsEvent);

        /// <inheritdoc />
        public string EventId { get; set; }

        /// <inheritdoc />
        public EventMetaData MetaData { get; set; } = new EventMetaData();
    }
}
