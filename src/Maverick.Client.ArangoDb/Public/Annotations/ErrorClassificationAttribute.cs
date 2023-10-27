using System;

namespace Maverick.Client.ArangoDb.Public.Annotations;

/// <summary>
///     Attribute to set severity of error code.
/// </summary>
public class ErrorClassificationAttribute : Attribute
{
    /// <summary>
    ///     Error severity of error code.
    /// </summary>
    public AErrorSeverity Severity { get; }

    /// <summary>
    ///     Create an instance of <see cref="ErrorClassificationAttribute" />.
    /// </summary>
    /// <param name="severity">Error severity of error code.</param>
    public ErrorClassificationAttribute(AErrorSeverity severity)
    {
        Severity = severity;
    }
}
