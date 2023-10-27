using System;

namespace Maverick.UserProfileService.AggregateEvents.Common
{
    /// <summary>
    ///     The meta data of an event.
    /// </summary>
    public class EventMetaData
    {
        /// <summary>
        ///     Information about the batch to which the event is assigned.
        /// </summary>
        public EventBatchData Batch { get; set; }

        /// <summary>
        ///     An unique identifier to link series of events.
        /// </summary>
        public string CorrelationId { set; get; }

        /// <summary>
        ///     Flag defines, if an event has to be inverted
        ///     from the projection.
        /// </summary>
        public bool HasToBeInverted { set; get; } = false;

        /// <summary>
        ///     Some user or service that initiates the event.
        /// </summary>
        public EventInitiator Initiator { get; set; }

        /// <summary>
        ///     The id of the process that initiated the event.
        ///     Hint: The correlation id cannot be used,
        ///     because several processes can use the same correlation id.
        /// </summary>
        public string ProcessId { set; get; }

        /// <summary>
        ///     Represents the stream name of the entity the current event belongs to.
        ///     The event affects the entity directly or indirectly. Direct manipulation would a modification of one or more
        ///     properties of the entity or a changed assignment from/to the entity itself.
        ///     Indirect manipulation means that some changes has been done in the tree (an assignment has been added
        ///     or remove above the entity) an the entity has to be informed because it has to know the tree above.
        /// </summary>
        public string RelatedEntityId { get; set; }

        /// <summary>
        ///     The timestamp when the event occurred.
        /// </summary>
        public DateTime Timestamp { set; get; }

        /// <summary>
        ///     Information about the used version of this event.
        /// </summary>
        public long? VersionInformation { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Meta data:{Environment.NewLine}"
                + $"   related entity id: {RelatedEntityId}{Environment.NewLine}"
                + $"   time stamp: {Timestamp}{Environment.NewLine}"
                + $"   correlation id: {CorrelationId}{Environment.NewLine}"
                + $"   process id: {ProcessId}{Environment.NewLine}"
                + $"   version: {VersionInformation:D}";
        }
    }
}
