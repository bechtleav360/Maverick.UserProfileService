using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;

namespace UserProfileService.Common.V2.CommandLineTools;

/// <summary>
///     Component that can output data to the configured console.
/// </summary>
public interface IOutput : ILogger
{
    /// <summary>
    ///     Field indicating if the Error-Stream is redirected from the console.
    /// </summary>
    bool IsErrorRedirected { get; }

    /// <summary>
    ///     Field indicating if the Input-Stream is redirected from the console.
    /// </summary>
    bool IsInputRedirected { get; }

    /// <summary>
    ///     Field indicating if the Output-Stream is redirected from the console.
    /// </summary>
    bool IsOutputRedirected { get; }

    /// <summary>
    ///     Field indicating current configured LogLevel.
    /// </summary>
    LogLevel LogLevel { get; set; }

    /// <summary>
    ///     Tuple of all current Redirections.
    /// </summary>
    (bool In, bool Out, bool Err) Redirections { get; }

    /// <summary>
    ///     Write a simple string at the given level and color to Stdout.
    /// </summary>
    /// <param name="str">The string that should be written in the console.</param>
    /// <param name="level">The level the string should be have.</param>
    /// <param name="color">The color that the output on the console should have.</param>
    void Write(string str, int level = 0, ConsoleColor color = ConsoleColor.White);

    /// <summary>
    ///     Write a Stream of data to Stdout.
    /// </summary>
    /// <param name="stream"></param>
    void Write(Stream stream);

    /// <summary>
    ///     Write a simple string at the given level and color to Stderr.
    /// </summary>
    /// <param name="str">The string that should be written in the console.</param>
    /// <param name="level">The level the string should be have.</param>
    /// <param name="color">The color that the output on the console should have.</param>
    void WriteError(string str, int level = 0, ConsoleColor color = ConsoleColor.Red);

    /// <summary>
    ///     Write a Stream of data to stderr.
    /// </summary>
    /// <param name="stream"></param>
    void WriteError(Stream stream);

    /// <summary>
    ///     Write a string followed by a newline at the given level and color to Stderr.
    /// </summary>
    /// <param name="str">The string that should be written in the console.</param>
    /// <param name="level">The level the string should be have.</param>
    /// <param name="color">The color that the output on the console should have.</param>
    void WriteErrorLine(string str, int level = 0, ConsoleColor color = ConsoleColor.Red);

    /// <summary>
    ///     Write a separator to stderr.
    /// </summary>
    void WriteErrorSeparator();

    /// <summary>
    ///     Write a string followed by a newline at the given level and color to Stdout.
    /// </summary>
    /// <param name="str">The string that should be written in the console.</param>
    /// <param name="level">The level the string should be have.</param>
    /// <param name="color">The color that the output on the console should have.</param>
    void WriteLine(string str, int level = 0, ConsoleColor color = ConsoleColor.White);

    /// <summary>
    ///     Write a Separator to Stdout.
    /// </summary>
    void WriteSeparator();

    /// <summary>
    ///     Write a table of Data to Stdout.
    /// </summary>
    /// <typeparam name="T">type of Object were operating on</typeparam>
    /// <param name="items">list of items, evaluated once using <paramref name="propertySelector" /></param>
    /// <param name="propertySelector">function returning a dictionary with all necessary properties for the table</param>
    void WriteTable<T>(IEnumerable<T> items, Func<T, Dictionary<string, object>> propertySelector);

    /// <summary>
    ///     Write a table of Data to Stdout.
    /// </summary>
    /// <typeparam name="T">type of Object were operating on</typeparam>
    /// <param name="items">List of items, evaluated once using <paramref name="propertySelector" /></param>
    /// <param name="propertySelector">function returning a dictionary with all necessary properties for the table</param>
    /// <param name="cellAlignments">mapping of column => TextAlign</param>
    void WriteTable<T>(
        IEnumerable<T> items,
        Func<T, Dictionary<string, object>> propertySelector,
        Dictionary<string, TextAlign> cellAlignments);

    /// <summary>
    ///     Write a string with the given level and color to stdout.
    /// </summary>
    /// <param name="str">The string that should be written in the console.</param>
    /// <param name="level">The level the string should be have.</param>
    /// <param name="color">The color that the output on the console should have.</param>
    void WriteVerbose(string str, int level = 0, ConsoleColor color = ConsoleColor.White);

    /// <summary>
    ///     Write a Stream of data to Stdout
    /// </summary>
    /// <param name="stream"></param>
    void WriteVerbose(Stream stream);

    /// <summary>
    ///     Write a string followed by a newline at the given level and color to stdout.
    /// </summary>
    /// <param name="str">The string that should be written in the console.</param>
    /// <param name="level">The level the string should be have.</param>
    /// <param name="color">The color that the output on the console should have.</param>
    void WriteVerboseLine(string str, int level = 0, ConsoleColor color = ConsoleColor.White);
}
