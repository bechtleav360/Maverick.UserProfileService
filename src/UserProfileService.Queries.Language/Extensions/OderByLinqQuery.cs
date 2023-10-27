using UserProfileService.Queries.Language.ExpressionOperators;
using UserProfileService.Queries.Language.Models;

namespace UserProfileService.Queries.Language.Extensions;

/// <summary>
///     Extension to create the order by query for the database.
/// </summary>
public static class OrderByLinqQueryExtension
{
    /// <summary>
    ///     An list of <see cref="SortedProperty" /> and creates out of this an string array
    ///     of order by request for the marten database.
    /// </summary>
    /// <param name="sortedProperties">
    ///     The list of <see cref="SortedProperty" /> that should be transformed into an array
    ///     of order by requests.
    /// </param>
    /// <returns>An array of string that contains the order by requests.</returns>
    /// <exception cref="ArgumentNullException">
    ///     If the
    ///     <paramref name="sortedProperties" />
    ///     is null.
    /// </exception>
    public static string[] CreateOrderByLinqQuery(this IEnumerable<SortedProperty>? sortedProperties)
    {
        if (sortedProperties == null)
        {
            throw new ArgumentNullException(nameof(sortedProperties));
        }

        IEnumerable<SortedProperty> properties = sortedProperties as SortedProperty[] ?? sortedProperties.ToArray();

        if (!properties.Any())
        {
            return Array.Empty<string>();
        }

        return properties
            .Where(p => !string.IsNullOrWhiteSpace(p.PropertyName) && p.Sorted != SortDirection.None)
            .Select(p => $"{p.PropertyName} {(p.Sorted == SortDirection.Ascending ? "asc" : "desc")}")
            .ToArray();
    }
}
