using System;

namespace UserProfileService.Common.V2.Extensions;

/// <summary>
///     Class containing some extensions methods for the class string
/// </summary>
public static class StringExtensions
{
    /// <summary>
    ///     Returns true if the provided string is a valid url otherwise false.
    /// </summary>
    /// <param name="input"> The provided string that should be check </param>
    /// <returns> True if the provided string is a valid url </returns>
    public static bool IsValidUrl(this string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            throw new ArgumentException(nameof(input));
        }

        Uri uriResult;

        return Uri.TryCreate(input, UriKind.Absolute, out uriResult)
            && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}
