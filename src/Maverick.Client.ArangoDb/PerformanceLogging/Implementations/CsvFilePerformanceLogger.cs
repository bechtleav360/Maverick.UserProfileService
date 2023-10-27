using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Maverick.Client.ArangoDb.PerformanceLogging.Abstractions;
using Maverick.Client.ArangoDb.PerformanceLogging.Configuration;
using Maverick.Client.ArangoDb.Public;
using Maverick.Client.ArangoDb.Public.Models.Query;

namespace Maverick.Client.ArangoDb.PerformanceLogging.Implementations;

/// <summary>
///     Represents an implementation of <see cref="IPerformanceLogger" /> that logs to a CSV (comma-separated) text file.
/// </summary>
internal class CsvFilePerformanceLogger : IPerformanceLogger
{
    private readonly bool _addFirstLine;
    private readonly string _pathAndFilename;
    private readonly string _separator;

    private static IEnumerable<string> ExtraHeaderValues =>
        new[]
        {
            "Execution time in sec. (Arango internal)",
            "Writes executed",
            "Writes ignored",
            "Scanned full",
            "Scanned index",
            "Filtered",
            "Peak memory usage",
            "Http requests"
        };

    private static IEnumerable<string> HeaderValues =>
        new[] { "Timestamp", "AQL", "Execution time (in sec.)", "Amount elements", "Transaction id" };

    /// <summary>
    ///     Initiates a new instance of <see cref="CsvFilePerformanceLogger" /> with a specified name for the resulting file
    ///     and a separator string.
    /// </summary>
    /// <param name="performanceLogSettings">
    ///     Contains all settings for this instance. Must be of type
    ///     <see cref="CsvPerformanceLogSettings" />.
    /// </param>
    public CsvFilePerformanceLogger(IPerformanceLogSettings performanceLogSettings)
    {
        if (!(performanceLogSettings is CsvPerformanceLogSettings settings))
        {
            throw new NotSupportedException(
                $"The provided settings are not supported by this implementation ('{nameof(CsvFilePerformanceLogger)}'). Got '{performanceLogSettings?.GetType().Name}', but expected '{nameof(CsvPerformanceLogSettings)}'.");
        }

        if (string.IsNullOrWhiteSpace(settings.Filename))
        {
            throw new ArgumentException(
                "The configured file name is invalid, because it is empty or whitespace.",
                nameof(performanceLogSettings));
        }

        string path = string.IsNullOrWhiteSpace(settings.Path)
            ? Directory.GetCurrentDirectory()
            : settings.Path;

        if (!Directory.Exists(path))
        {
            try
            {
                Directory.CreateDirectory(path);
            }
            catch (Exception e)
            {
                throw new Exception(
                    $"{nameof(CsvFilePerformanceLogger)}: Could not create performance log directory '{path}'. {e.Message}",
                    e);
            }
        }

        _pathAndFilename = Path.Combine(path, settings.Filename);

        _separator = string.IsNullOrWhiteSpace(settings.Separator)
            ? ","
            : settings.Separator.Trim();

        _addFirstLine = settings.AddExcelSeparatorInfo;
    }

    private async Task<StreamWriter> CreateFileStream()
    {
        if (!File.Exists(_pathAndFilename))
        {
            StreamWriter stream = File.CreateText(_pathAndFilename);

            if (_addFirstLine)
            {
                await stream.WriteLineAsync($"sep={_separator}");
            }

            await stream.WriteLineAsync(
                string.Join(_separator, HeaderValues.Concat(ExtraHeaderValues))); // header of CSV

            return stream;
        }

        return File.AppendText(_pathAndFilename);
    }

    private static IEnumerable<string> GetExtraInformation(CursorResponseExtra input)
    {
        if (input?.Stats == null)
        {
            yield break;
        }

        yield return input.Stats.ExecutionTime.ToString("F6", CultureInfo.InvariantCulture);
        yield return input.Stats.WritesExecuted.ToString();
        yield return input.Stats.WritesIgnored.ToString();
        yield return input.Stats.ScannedFull.ToString();
        yield return input.Stats.ScannedIndex.ToString();
        yield return input.Stats.Filtered.ToString();
        yield return input.Stats.PeakMemoryUsage.ToString();
        yield return input.Stats.HttpRequests.ToString();
    }

    private static string EscapeAqlQueryString(string input)
    {
        if (input == null)
        {
            return string.Empty;
        }

        string first = input
            .Trim('"')
            .Replace(Environment.NewLine, " ", StringComparison.OrdinalIgnoreCase)
            .Replace("\"", "\"\"");

        return string.Concat(
            "\"",
            Regex.Replace(first, "\\s+", " "),
            "\"");
    }

    /// <inheritdoc cref="IPerformanceLogger" />
    public async Task LogAsync(
        CreateCursorBody request,
        ICursorInnerResponse response,
        TimeSpan executionTime,
        DateTime timestamp,
        string transactionId = null)
    {
        if (request == null)
        {
            return;
        }

        await using StreamWriter writer = await CreateFileStream();

        string[] args = new[]
            {
                timestamp.ToString("s"),
                EscapeAqlQueryString(request.Query),
                executionTime.TotalSeconds.ToString("F6", CultureInfo.InvariantCulture),
                response?.Count.ToString() ?? string.Empty,
                transactionId ?? string.Empty
            }
            .Concat(GetExtraInformation(response?.Extra))
            .ToArray();

        await writer.WriteLineAsync(string.Join(_separator, args));
    }
}
