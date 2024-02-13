using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace UserProfileService.Api.Common.Extensions;

/// <summary>
///     Extensions for <see cref="string" />
/// </summary>
public static class StringExtensions
{
    /// <summary>
    ///     Formats a string with the properties of an object
    /// </summary>
    /// <param name="format">String to format</param>
    /// <param name="source">Source object for properties</param>
    /// <returns>Formatted string</returns>
    public static string FormatWith(this string format, object source)
    {
        if (string.IsNullOrEmpty(format))
        {
            throw new ArgumentException("A value of parameter must be set.", nameof(format));
        }

        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        var sbResult = new StringBuilder(format.Length);
        var sbCurrentTerm = new StringBuilder();
        char[] formatChars = format.ToCharArray();
        var inTerm = false;
        object? currentPropValue = source;

        for (var i = 0; i < format.Length; i++)
        {
            if (formatChars[i] == '{')
            {
                inTerm = true;
            }
            else if (formatChars[i] == '}')
            {
                PropertyInfo? pi = currentPropValue?.GetType().GetProperty(sbCurrentTerm.ToString());

                if (pi == null)
                {
                    continue;
                }

                MethodInfo? methodInfo = pi.PropertyType.GetMethod(nameof(ToString), Array.Empty<Type>());
                if (methodInfo == null)
                {
                    continue;
                }

                sbResult.Append((string?)methodInfo
                    .Invoke(pi.GetValue(currentPropValue, null), null));

                sbCurrentTerm.Clear();
                inTerm = false;
                currentPropValue = source;
            }
            else if (inTerm)
            {
                if (formatChars[i] == '.')
                {
                    PropertyInfo? pi = currentPropValue?.GetType().GetProperty(sbCurrentTerm.ToString());
                    currentPropValue = pi?.GetValue(source, null);
                    sbCurrentTerm.Clear();
                }
                else
                {
                    sbCurrentTerm.Append(formatChars[i]);
                }
            }
            else
            {
                sbResult.Append(formatChars[i]);
            }
        }

        return sbResult.ToString();
    }

    public static string StringFormat(string format, IDictionary<string, string> values)
    {
        MatchCollection matches = Regex.Matches(format, @"\{(.+?)\}");
        List<string> words = (from Match match in matches select match.Groups[1].Value).ToList();

        return words.Aggregate(
            format,
            (current, key) =>
            {
                int colonIndex = key.IndexOf(':');

                return current.Replace(
                    "{" + key + "}",
                    colonIndex > 0
                        ? values[key[..colonIndex]]
                        : values[key]);
            });
    }

    /// <summary>
    ///     Remove the given string at the end of the current string.
    /// </summary>
    /// <param name="s">The current string</param>
    /// <param name="stringToTrim">The given string which should be removed at the end of the current string</param>
    /// <returns>The modified string</returns>
    public static string TrimEnd(this string s, string stringToTrim)
    {

        if (stringToTrim != null && s.EndsWith(stringToTrim))
        {
            return s.Substring(0, s.Length - stringToTrim.Length);
        }

        return s;
    }
}
