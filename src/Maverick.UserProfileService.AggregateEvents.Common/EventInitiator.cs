namespace Maverick.UserProfileService.AggregateEvents.Common
{
    /// <summary>
    ///     Some user or service that initiates or publish the message.
    /// </summary>
    public class EventInitiator
    {
        /// <summary>
        ///     Identifier of initiator for the event.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        ///     Returns the default System-<see cref="EventInitiator" />.
        /// </summary>
        public static EventInitiator SystemInitiator =>
            new EventInitiator
            {
                Id = "system",
                Type = InitiatorType.System
            };

        /// <summary>
        ///     Defines the type of the initiator that initiates or publish the message
        /// </summary>
        public InitiatorType Type { get; set; }
    }
}
