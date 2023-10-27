using UserProfileService.Queries.Language.ExpressionOperators;

namespace UserProfileService.Queries.Language.Models;

/// <summary>
///     The sorted property to store the property name and
///     with which sort direction they should be ordered.
/// </summary>
public record SortedProperty
{
    /// <summary>
    ///     The name of the property that a sort should be made.
    /// </summary>
    public string PropertyName { get; }

    /// <summary>
    ///     The sort direction that should be apply.
    /// </summary>
    public SortDirection Sorted { get; }

    /// <summary>
    ///     Creates a object of type <see cref="SortedProperty" />.
    /// </summary>
    /// <param name="propertyName">The name of the property that a sort should be made.</param>
    /// <param name="sorted">The sort direction that should be apply.</param>
    public SortedProperty(string propertyName, SortDirection sorted)
    {
        PropertyName = propertyName;
        Sorted = sorted;
    }
}
