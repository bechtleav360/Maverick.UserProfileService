using System;

namespace UserProfileService.Common.V2.Exceptions;

/// <summary>
///     The exception that is thrown when the event number of the projection service does not match the number in the
///     database.
/// </summary>
public class ProjectionMissMatchException : Exception
{
    /// <summary>
    ///     The current event number to handle.
    /// </summary>
    public ulong EventNumber { get; }

    /// <summary>
    ///     The expected event number to handle.
    /// </summary>
    public ulong ExpectedEventNumber { get; }

    /// <summary>
    ///     Name of the projection service.
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     Create an instance of <see cref="ProjectionMissMatchException" />
    /// </summary>
    /// <param name="message">The related error message.</param>
    /// <param name="name">Name of the projection service.</param>
    /// <param name="expectedEventNumber">The expected event number to handle.</param>
    /// <param name="eventNumber">The current event number to handle.</param>
    public ProjectionMissMatchException(
        string message,
        string name,
        ulong expectedEventNumber,
        ulong eventNumber) : base(message)
    {
        Name = name;
        ExpectedEventNumber = expectedEventNumber;
        EventNumber = eventNumber;
    }
}
