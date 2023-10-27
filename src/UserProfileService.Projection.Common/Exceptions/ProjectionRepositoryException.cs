using System;

namespace UserProfileService.Projection.Common.Exceptions;

/// <summary>
///     Will be thrown when a projection repository failed to process a request properly.
/// </summary>
public class ProjectionRepositoryException : Exception
{
    /// <summary>
    ///     Initializes a new instance of <see cref="ProjectionRepositoryException" /> with a specified error
    ///     <paramref name="message" />.
    /// </summary>
    /// <param name="message">The error message.</param>
    public ProjectionRepositoryException(string message) : base(message)
    {
    }

    /// <summary>
    ///     Initializes a new instance of <see cref="ProjectionRepositoryException" /> with a specified error
    ///     <paramref name="message" /> and a reference to the exception that caused the error.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">Reference to the exception that caused the error.</param>
    public ProjectionRepositoryException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
