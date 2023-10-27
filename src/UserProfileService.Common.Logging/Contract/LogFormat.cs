namespace UserProfileService.Common.Logging.Contract;

/// <summary>
///     The log format
/// </summary>
public enum LogFormat
{
    /// <summary>
    ///     Renders log messages as json objects (structured logging)
    /// </summary>
    Json,

    /// <summary>
    ///     Renders log messages as plain text
    /// </summary>
    Text
}
