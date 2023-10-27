using System;
using UserProfileService.Common.V2.Enums;

namespace UserProfileService.Common.V2.Models;

/// <summary>
///     Response for schema initialization process.
/// </summary>
public class SchemaInitializationResponse
{
    /// <summary>
    ///     The exception when an error occurs during the schema initialization process.
    /// </summary>
    public Exception Exception { get; set; }

    /// <summary>
    ///     Status of the schema initialization process.
    /// </summary>
    public SchemaInitializationStatus Status { get; set; }

    /// <summary>
    ///     Create an instance of <see cref="SchemaInitializationResponse" />.
    /// </summary>
    /// <param name="status">Status of the schema initialization process.</param>
    /// <param name="exception">The exception when an error occurs during the schema initialization process.</param>
    public SchemaInitializationResponse(SchemaInitializationStatus status, Exception exception = null)
    {
        Status = status;
        Exception = exception;
    }
}
