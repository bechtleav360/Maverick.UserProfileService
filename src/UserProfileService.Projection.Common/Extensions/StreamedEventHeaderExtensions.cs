using System;
using Maverick.UserProfileService.Models.Models;
using UserProfileService.EventSourcing.Abstractions.Models;

namespace UserProfileService.Projection.Common.Extensions;

/// <summary>
///     Contains extension methods for <see cref="StreamedEventHeader" /> instances.
/// </summary>
public static class StreamedEventHeaderExtensions
{
    /// <summary>
    ///     Converts a <see cref="StreamedEventHeader" /> to a <see cref="ProjectionState" />.
    /// </summary>
    /// <param name="header">The header instance to be converted.</param>
    /// <param name="exception">
    ///     An optional parameter that passes a reference to an occurred exception and that should be
    ///     mentioned in the new <see cref="ProjectionState" />. If it is not provided, it will be assumed that no error has
    ///     been occurred.
    /// </param>
    /// <param name="processedOn">
    ///     An optional <see cref="DateTimeOffset" /> marking the time when the event has been processed.
    ///     If it is not provided, the current UTC time will be taken.
    /// </param>
    /// <param name="processingStarted">
    ///     An optional <see cref="DateTimeOffset" /> marking the time when the processing of the event has begun.
    ///     If it is not provided, it will remain null.
    /// </param>
    /// <returns>
    ///     A new instance of <see cref="ProjectionState" /> containing information from <paramref name="header" />. if
    ///     <paramref name="header" /> is <c>null</c>, <c>null</c> will be returned as well.
    /// </returns>
    public static ProjectionState ToProjectionState(
        this StreamedEventHeader header,
        Exception exception = null,
        DateTimeOffset? processedOn = null,
        DateTimeOffset? processingStarted = null)
    {
        if (header == null)
        {
            return default;
        }

        return new ProjectionState
        {
            EventId = header.EventId.ToString("D"),
            EventNumberVersion = header.EventNumberVersion,
            EventNumberSequence = header.EventNumberSequence,
            ProcessedOn = processedOn
                ?? DateTimeOffset.UtcNow,
            ProcessingStartedAt = processingStarted,
            EventName = header.EventType,
            StreamName = header.EventStreamId,
            ErrorMessage = exception?.Message,
            ErrorOccurred = exception != null,
            StackTraceMessage = exception?.ToString()
        };
    }
}
