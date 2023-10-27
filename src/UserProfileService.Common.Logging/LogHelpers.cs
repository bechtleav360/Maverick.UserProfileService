using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading;
using Maverick.UserProfileService.Models.Models;

namespace UserProfileService.Common.Logging;

/// <summary>
///     Contains helping methods used in loggers or to log messages.
/// </summary>
public static class LogHelpers
{
    /// <summary>
    ///     Converts several input arguments to an object array (can be helpful for logging messages).
    /// </summary>
    /// <param name="arguments">The arguments to be combined in an object array.</param>
    /// <returns>The object array containing all <paramref name="arguments" />.</returns>
    public static object[] Arguments(params object[] arguments)
    {
        return arguments;
    }

    /// <summary>
    ///     Stores a provided object into an object array and returns the array (can be helpful for logging messages).
    /// </summary>
    /// <param name="argument">The object to be wrapped.</param>
    /// <returns>The object array containing the <paramref name="argument" />.</returns>
    public static object[] AsArgumentList(this object argument)
    {
        return new[] { argument };
    }

    /// <summary>
    ///     Returns the string representation of an provided object usable for logging messages.
    /// </summary>
    /// <param name="input">The object to be presented as single string.</param>
    /// <returns>The output string.</returns>
    public static string ToLogString(this object input)
    {
        if (input == null)
        {
            return string.Empty;
        }

        switch (input)
        {
            case bool b:
                return b.ToString();
            case int i:
                return i.ToString("D", CultureInfo.InvariantCulture);
            case long l:
                return l.ToString("D", CultureInfo.InvariantCulture);
            case float f:
                return f.ToString(CultureInfo.InvariantCulture);
            case double d:
                return d.ToString(CultureInfo.InvariantCulture);
            case byte[] bArray:
                return $"{bArray.Length} bytes";
            case DateTime dt:
                return dt.ToString("u");
            case DateTimeOffset dto:
                return dto.ToString("u");
            case string s:
                return s;
            case ObjectIdent s:
                return $"{s.Type}-{s.Id}";
            case Guid s:
                return s.ToString();
            case string[] stringArray:
                return string.Join(",", stringArray);
            case IList<string> stringList:
                return string.Join(",", stringList);
            case CancellationToken ct:
                return $"CancellationToken.IsCancellationRequested: {ct.IsCancellationRequested}";
            case Enum en:
                return en.ToString("G");
        }

        return JsonSerializer.Serialize(
            input,
            new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                IgnoreNullValues = true,
                WriteIndented = false,
                IgnoreReadOnlyProperties = true
            });
    }
}
