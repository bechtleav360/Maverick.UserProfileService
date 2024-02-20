using System;
using UserProfileService.Messaging.Annotations;

namespace UserProfileService.Sync.Messages
{
    /// <summary>
    ///     Defines a command for the start stopwatch in the analyzed state of the state machine.
    ///     Its aim is to track the change of the UpdateAt property in the saga object. The current step will be skipped
    ///     if there is a longer standstill. It's useful to check, if the saga worker doesn't respond anymore.
    /// </summary>
    [Message(ServiceName = "sync", ServiceGroup = "user-profile")]
    public class TriggerStopWatchCommand
    {
        /// <summary>
        ///     The correlation id of the sender who sent the command to uniquely assign the response.
        ///     Not to be confused with the correlation id,
        ///     which is sent in the header of the message and is passed along by the individual services.
        /// </summary>
        public Guid? CorrelationId { get; set; }

        /// <summary>
        ///     The Id of the current collecting process
        /// </summary>
        public Guid CollectingId { get; set; }

    }
}
