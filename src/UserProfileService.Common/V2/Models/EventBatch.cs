using System;
using UserProfileService.Common.V2.Enums;

namespace UserProfileService.Common.V2.Models;

/// <summary>
///     Batch of events.
/// </summary>
public class EventBatch
{
    /// <summary>
    ///     Datetime when the batch was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    ///     Id of batch.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    ///     Status of batch.
    /// </summary>
    public EventStatus Status { get; set; }

    /// <summary>
    ///     Datetime when the batch was updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    ///     A descriptive message about the last occurred error.
    /// </summary>
    public string LastErrorMessage { get; set; }
}
