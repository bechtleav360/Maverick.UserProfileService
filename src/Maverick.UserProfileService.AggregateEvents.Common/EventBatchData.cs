using System;

namespace Maverick.UserProfileService.AggregateEvents.Common
{
    /// <summary>
    ///     Information about the batch to which the event is assigned.
    /// </summary>
    public class EventBatchData
    {
        /// <summary>
        ///     Index of current event related to all events. Starting at 1
        /// </summary>
        public int Current { get; set; }

        /// <summary>
        ///     Id of the related batch of the event tuple.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        ///     Number of events associated with the batch.
        /// </summary>
        public int Total { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Meta data:{Environment.NewLine}"
                + $"   id: {Id}{Environment.NewLine}"
                + $"   current: {Current}{Environment.NewLine}"
                + $"   total: {Total}{Environment.NewLine}";
        }
    }
}
