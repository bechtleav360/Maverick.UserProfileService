using System;
using System.Threading.Tasks;

namespace Maverick.Client.ArangoDb.Public.Configuration;

/// <summary>
///     Contains configuration for exception handling within arango.
/// </summary>
public class ArangoExceptionConfiguration
{
    /// <summary>
    ///     The duration the circuit will stay open before resetting.
    /// </summary>
    public TimeSpan DurationOfBreak { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    ///     Handler, which will be executed if an error occurred.
    /// </summary>
    public Func<Exception, Task> ExceptionHandler { get; set; }

    /// <summary>
    ///     The number of exceptions or handled results that are allowed before opening the circuit
    /// </summary>
    public int HandledEventsAllowedBeforeBreaking { get; set; } = 5;

    /// <summary>
    ///     The retry count if request failed.
    /// </summary>
    public int RetryCount { get; set; } = 3;

    /// <summary>
    ///     Specifies whether a retry should be attempted in the event of an error.
    /// </summary>
    public bool RetryEnabled { get; set; } = true;

    /// <summary>
    ///     The function that provides the duration to wait for for a particular retry attempt.
    /// </summary>
    public Func<int, TimeSpan> SleepDuration { get; set; } = i => TimeSpan.FromSeconds(1);
}
