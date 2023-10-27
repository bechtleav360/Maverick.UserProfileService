using System;

namespace UserProfileService.Common.V2.Exceptions;

/// <summary>
///     The exception is thrown if an object differs from the expected version.
/// </summary>
public class ObjectVersionMismatchException<T> : Exception
{
    /// <summary>
    ///     Version that was expected
    /// </summary>
    public T ExpectedVersion { get; set; }

    /// <summary>
    ///     Version that was found
    /// </summary>
    public T OldVersion { get; set; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ObjectVersionMismatchException{T}" /> class.
    /// </summary>
    public ObjectVersionMismatchException()
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ObjectVersionMismatchException{T}" /> class.
    /// </summary>
    /// <param name="message">
    ///     <see cref="Exception.Message" />
    /// </param>
    public ObjectVersionMismatchException(string message) : base(message)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ObjectVersionMismatchException{T}" /> class.
    /// </summary>
    /// <param name="message">
    ///     <see cref="Exception.Message" />
    /// </param>
    /// <param name="oldVersion">Version found.</param>
    /// <param name="expectedVersion">Version expected.</param>
    public ObjectVersionMismatchException(string message, T oldVersion, T expectedVersion) : this(message)
    {
        OldVersion = oldVersion;
        ExpectedVersion = expectedVersion;
    }
}
