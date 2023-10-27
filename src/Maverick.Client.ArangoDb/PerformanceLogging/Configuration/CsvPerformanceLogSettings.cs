using System;
using Maverick.Client.ArangoDb.PerformanceLogging.Abstractions;
using Maverick.Client.ArangoDb.PerformanceLogging.Implementations;

namespace Maverick.Client.ArangoDb.PerformanceLogging.Configuration;

/// <summary>
///     Contains properties to set up CSV performance loggers.
/// </summary>
public class CsvPerformanceLogSettings : IPerformanceLogSettings
{
    /// <summary>
    ///     Can be set to <c>true</c>, if "sep=," should be added as first line.
    /// </summary>
    public bool AddExcelSeparatorInfo { get; set; }

    /// <summary>
    ///     The name of the resulting text file.
    /// </summary>
    public string Filename { get; set; }

    /// <summary>
    ///     The path the text file will be stored to.
    /// </summary>
    public string Path { get; set; }

    /// <summary>
    ///     The string to be used to separate columns.
    /// </summary>
    public string Separator { get; set; }

    /// <inheritdoc cref="IPerformanceLogSettings" />
    public string Type => "csv";

    /// <inheritdoc cref="IPerformanceLogSettings" />
    public Type GetImplementationType()
    {
        return typeof(CsvFilePerformanceLogger);
    }
}
