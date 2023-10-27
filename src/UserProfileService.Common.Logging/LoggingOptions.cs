using UserProfileService.Common.Logging.Contract;

namespace UserProfileService.Common.Logging;

/// <summary>
///     Additional logging options
/// </summary>
public class LoggingOptions
{
    /// <summary>
    ///     Enables log output to a file
    /// </summary>
    public bool EnableLogFile { get; set; }

    /// <summary>
    ///     The maximum number of files to store (default: 3)
    /// </summary>
    public int LogFileMaxHistory { get; set; } = 3;

    /// <summary>
    ///     The file log path without trailing slash (default: logs)
    /// </summary>
    public string LogFilePath { get; set; } = "logs";

    /// <summary>
    ///     The log format to use (default: Json)
    /// </summary>
    public LogFormat LogFormat { get; set; } = LogFormat.Json;
}
