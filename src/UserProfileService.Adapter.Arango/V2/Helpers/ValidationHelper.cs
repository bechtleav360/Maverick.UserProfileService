using System;
using System.Collections.Generic;
using System.Linq;

namespace UserProfileService.Adapter.Arango.V2.Helpers;

/// <summary>
///     Contains methods that will check, if objects are valid or not.
/// </summary>
public static class ValidationHelper
{
    /// <summary>
    ///     Return <c>true</c>, if the collection is either null or empty.
    /// </summary>
    /// <typeparam name="TElem">Type of collection elements.</typeparam>
    /// <param name="collection">The collection to be checked.</param>
    /// <returns>Boolean value indicating whether the collection is null/empty or not.</returns>
    internal static bool IsNullOrEmpty<TElem>(IList<TElem> collection)
    {
        return collection == null || collection.Count == 0;
    }

    /// <summary>
    ///     Checks if <paramref name="parameterValue" /> of <paramref name="parameterName" /> is <c>null</c> or empty or if it
    ///     is only whitespace.
    /// </summary>
    /// <param name="parameterValue">Value of the parameter.</param>
    /// <param name="parameterName">Name of parameter.</param>
    /// <exception cref="ArgumentException">If parameter value is invalid.</exception>
    /// <exception cref="ArgumentNullException">If parameter value is null.</exception>
    public static void CheckParameter(
        string parameterValue,
        string parameterName)
    {
        if (parameterValue == null)
        {
            throw new ArgumentNullException(parameterName);
        }

        if (string.IsNullOrWhiteSpace(parameterValue))
        {
            throw new ArgumentException($"'{parameterName}' cannot be empty or whitespace.", parameterName);
        }
    }

    /// <summary>
    ///     Checks if <paramref name="parameterValue" /> of <paramref name="parameterName" /> is <c>null</c> or empty.
    /// </summary>
    /// <param name="parameterValue">Value of the parameter.</param>
    /// <param name="parameterName">Name of parameter.</param>
    /// <exception cref="ArgumentException">If parameter value is empty.</exception>
    /// <exception cref="ArgumentNullException">If parameter value is null.</exception>
    public static void CheckIfParameterIsNullOrEmpty<T>(
        IEnumerable<T> parameterValue,
        string parameterName)
    {
        if (parameterValue == null)
        {
            throw new ArgumentNullException(parameterName);
        }

        if (!parameterValue.Any())
        {
            throw new ArgumentException($"'{parameterName}' cannot be empty.", parameterName);
        }
    }
}
