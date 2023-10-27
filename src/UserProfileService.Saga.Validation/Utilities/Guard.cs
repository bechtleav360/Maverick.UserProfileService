using System;

namespace UserProfileService.Saga.Validation.Utilities;

/// <summary>
///     Guard to check values.
/// </summary>
public static class Guard
{
    /// <summary>
    ///     Checks if the given object is not null.
    /// </summary>
    /// <param name="obj">Object to check.</param>
    /// <param name="paramName">Name of related argument to check</param>
    /// <exception cref="ArgumentNullException">Exception if object is null.</exception>
    public static void IsNotNull(object obj, string paramName = null)
    {
        if (obj == null)
        {
            throw new ArgumentNullException(paramName, $"{paramName} cannot be null");
        }
    }

    /// <summary>
    ///     Checks if the given string is not null, empty or whitespace.
    /// </summary>
    /// <param name="str">String to check.</param>
    /// <param name="paramName">Name of related argument to check.</param>
    /// <exception cref="ArgumentNullException">Exception if string is null.</exception>
    /// <exception cref="ArgumentException">Exception if string is empty or whitespace.</exception>
    public static void IsNotNullOrEmpty(string str, string paramName = null)
    {
        IsNotNull(str, paramName);
        IsNotEmpty(str, paramName);
    }

    /// <summary>
    ///     Checks if the given string is not empty or whitespace.
    /// </summary>
    /// <param name="str">String to check.</param>
    /// <param name="paramName">Name of related argument to check.</param>
    /// <exception cref="ArgumentException">Exception if string is empty or whitespace.</exception>
    public static void IsNotEmpty(string str, string paramName = null)
    {
        if (string.IsNullOrEmpty(str) || string.IsNullOrWhiteSpace(str))
        {
            throw new ArgumentException(paramName, $"{paramName} cannot be empty");
        }
    }
}
