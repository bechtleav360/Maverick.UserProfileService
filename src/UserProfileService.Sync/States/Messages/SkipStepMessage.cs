using System;
using UserProfileService.Sync.Abstractions;

namespace UserProfileService.Sync.States.Messages
{

    /// <summary>
    ///     This message is emitted when a step inside the sync process should be skipped
    /// </summary>
    public class SkipStepMessage : ISyncMessage
    {
        /// <inheritdoc />
        public Guid Id { get; set; }

        /// <summary>
        ///     Collection id of step to collecting response.
        /// </summary>
        public Guid? CollectingId { get; set; }

    }
}
