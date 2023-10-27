using System;

namespace Maverick.UserProfileService.Models.Models
{
    /// <summary>
    ///     The event id model that stores the last event number
    ///     in the database.
    /// </summary>
    public class ProjectionState
    {
        /// <summary>
        ///     Stored an error message if one occurred.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        ///     True if an error occurred, otherwise false.
        /// </summary>
        public bool ErrorOccurred { set; get; }

        /// <summary>
        ///     The event id that was processed.
        /// </summary>
        public string EventId { set; get; }

        /// <summary>
        ///     The name of the processed event.
        /// </summary>
        public string EventName { set; get; }

        /// <summary>
        ///     The last event number in the stream that was process by the projection.
        /// </summary>
        public long EventNumberSequence { set; get; }

        /// <summary>
        ///     The last event number in the stream that was process by the projection.
        /// </summary>
        public long EventNumberVersion { set; get; }

        /// <summary>
        ///     When the event was processed.
        /// </summary>
        public DateTimeOffset ProcessedOn { set; get; }

        /// <summary>
        ///     When the processing of the event started.
        /// </summary>
        public DateTimeOffset? ProcessingStartedAt { set; get; }

        /// <summary>
        ///     Stored an error message of the inner exception if one occurred.
        /// </summary>
        public string StackTraceMessage { set; get; }

        /// <summary>
        ///     The name of the stream the current event belongs to.
        /// </summary>
        public string StreamName { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return
                $"EvenNumberVersion: {EventNumberVersion}, EventNumberSequence:{EventNumberSequence} - ProcessOn: {ProcessingStartedAt} - {ProcessedOn.Date}, EventName: {EventName}, EventId {EventId}, ErrorMessage: {ErrorMessage}, StreamName: {StreamName}, ErrorOccurred: {ErrorOccurred}, StackTraceMessage: {StackTraceMessage}";
        }
    }
}
