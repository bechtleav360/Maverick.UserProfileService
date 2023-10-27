using System;

namespace UserProfileService.Common.V2.Utilities;

/// <summary>
///     Provider to provide shared validation logic for sync and saga worker.
/// </summary>
public static class ValidationLogicProvider
{
    private static bool CompareStringByIgnoreCase(string first, string second)
    {
        return string.Equals(first, second, StringComparison.InvariantCultureIgnoreCase);
    }

    /// <summary>
    ///     Shared validation logic for groups.
    /// </summary>
    public static class Group
    {
        /// <summary>
        ///     Compare given names by ignoring case.
        ///     Name and display name are checked against each other.
        /// </summary>
        /// <param name="sourceName">Source name to compare.</param>
        /// <param name="sourceDisplayName">Source display name to compare.</param>
        /// <param name="targetName">Target name to compare.</param>
        /// <param name="targetDisplayName">Target display name to compare.</param>
        /// <returns>True if names are equal, otherwise false.</returns>
        public static bool CompareNames(
            string sourceName,
            string sourceDisplayName,
            string targetName,
            string targetDisplayName)
        {
            return
                CompareStringByIgnoreCase(sourceName, targetName)
                || CompareStringByIgnoreCase(sourceName, targetDisplayName)
                || CompareStringByIgnoreCase(sourceDisplayName, targetName)
                || CompareStringByIgnoreCase(sourceDisplayName, targetDisplayName);
        }
    }
}
