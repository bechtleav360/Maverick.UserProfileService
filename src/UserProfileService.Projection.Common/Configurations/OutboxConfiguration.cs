using System;

namespace UserProfileService.Projection.Common.Configurations;

/// <summary>
///     Configuration for outbox processor.
/// </summary>
internal class OutboxConfiguration
{
    /// <summary>
    ///     Interval (in milliseconds) when to check for batches that have not been executed.
    /// </summary>
    public TimeSpan Time { get; set; }
}
