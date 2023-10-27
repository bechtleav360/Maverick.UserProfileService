using System;
using System.Collections.Generic;
using Maverick.UserProfileService.Models.EnumModels;

namespace Maverick.UserProfileService.Models.Models
{
    /// <summary>
    ///     Activity log of an event.
    /// </summary>
    public class ActivityLogEntry
    {
        /// <summary>
        ///     Additional information that enriches the current event to ensure a user-friendly display.
        /// </summary>
        public IDictionary<string, object> AdditionalInformation = new Dictionary<string, object>();

        /// <summary>
        ///     The correlation id that is needed for logging purposes.
        /// </summary>
        public string CorrelationId { get; set; }

        /// <summary>
        ///     Id of the associated event.
        ///     Events can be split into different activity log entries
        ///     so that this id is not unique.
        /// </summary>
        public string EventId { get; set; }

        /// <summary>
        ///     Unique identifier of activity log entry.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        ///     Initiator who triggered the current command over event.
        /// </summary>
        public Initiator Initiator { get; set; }

        /// <summary>
        ///     Defines the current log entry as the leading entry.
        ///     To distinguish between main and secondary entries
        ///     when events are split into multiple entries.
        /// </summary>
        public bool Lead { get; set; }

        /// <summary>
        ///     Referenced id of the object to which the activity log belongs.
        ///     The type of the object is defined by the scope.
        /// </summary>
        public string ReferenceId { get; set; }

        /// <summary>
        ///     Scope of the activity log to which the event belongs.
        /// </summary>
        public ObjectType Scope { get; set; }

        /// <summary>
        ///     Time at which the event was processed.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        ///     Type of the event that is processed, on which the additional information depends.
        /// </summary>
        public string Type { get; set; }
    }
}
