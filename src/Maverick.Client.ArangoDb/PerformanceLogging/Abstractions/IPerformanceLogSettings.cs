using System;

namespace Maverick.Client.ArangoDb.PerformanceLogging.Abstractions;

/// <summary>
///     Marks a class as configuration class of an <see cref="IPerformanceLogger" />implementation.
/// </summary>
public interface IPerformanceLogSettings
{
    /// <summary>
    ///     Gets the type of settings instance (i.e. csv).
    /// </summary>
    string Type { get; }

    /// <summary>
    ///     Gets the implementation type the settings will be used for.
    /// </summary>
    Type GetImplementationType();
}
