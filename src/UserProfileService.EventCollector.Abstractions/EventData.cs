using System;

namespace UserProfileService.EventCollector.Abstractions;

/// <summary>
///     Data for collecting all events according to event collector.
/// </summary>
public class EventData
{
    /// <summary>
    ///     The id of the process for the events are collected.
    /// </summary>
    public Guid CollectingId { get; set; }

    /// <summary>
    ///     The data stored for the event and important for collecting for all events according to collector.
    /// </summary>
    public string Data { get; set; }

    /// <summary>
    ///     An error occurred during the consolidation process.
    /// </summary>
    public bool ErrorOccurred { get; set; } = false;

    /// <summary>
    ///     Defines the host of the event.
    /// </summary>
    public string Host { get; set; }

    /// <summary>
    ///     The Id used to assign event responses to event requests.
    /// </summary>
    public string RequestId { get; set; }
}
