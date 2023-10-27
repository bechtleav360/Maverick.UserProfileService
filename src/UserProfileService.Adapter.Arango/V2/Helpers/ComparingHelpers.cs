using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Maverick.UserProfileService.Models.EnumModels;
using UserProfileService.Adapter.Arango.V2.Abstractions;
using UserProfileService.Adapter.Arango.V2.Extensions;

namespace UserProfileService.Adapter.Arango.V2.Helpers;

internal static class ComparingHelpers
{
    /// <summary>
    ///     This method will only check, whether the both instance have the same reference.
    /// </summary>
    /// <remarks>
    ///     This method is used by <see cref="ArangoDbEnumerableExtensions" />. Therefore the order of input arguments is
    ///     important.
    ///     The filter arguments are only valid for <paramref name="one" />s that are sequences, lists, etc.
    /// </remarks>
    internal static bool SequenceEqual(
        IEnumerable one,
        IEnumerable two,
        string filterPropertyName = null,
        object filterPropertyValue = null)
    {
        return (ReferenceEquals(null, one) && ReferenceEquals(null, two))
            || (!ReferenceEquals(null, one) && !ReferenceEquals(null, two) && ReferenceEquals(one, two));
    }

    /// <summary>
    ///     This method deals as a placeholder for the ArangoDb query building process.<br />
    /// </summary>
    /// <remarks>
    ///     It checks, if any/all element(s) of sequence one is/are contained in sequence two.
    ///     The filter arguments are only valid for <paramref name="one" />s that are sequences, lists, etc.
    /// </remarks>
    internal static bool CheckExistenceOfElementsInTwoSequences(
        IEnumerable one,
        IEnumerable two,
        bool allIncluded,
        string filterPropertyName = null,
        object filterPropertyValue = null)
    {
        return false;
    }

    /// <summary>
    ///     The methods checks if <paramref name="two" /> is part of <paramref name="one" />.
    ///     <paramref name="one" /> should not be <see cref="IEnumerable" />.
    ///     This method will only check, whether the both instance have the same reference.
    /// </summary>
    /// <remarks>
    ///     This method is used by <see cref="ArangoDbEnumerableExtensions" />. Therefore the order of input arguments is
    ///     important.
    ///     The filter arguments are only valid for <paramref name="one" />s that are objects, lists, etc.
    /// </remarks>
    internal static bool CheckExistenceOfElements(
        object one,
        object two,
        FilterOperator @operator,
        bool allMustBeContained,
        string filterPropertyName = null,
        object filterPropertyValue = null)
    {
        return true;
    }

    /// <summary>
    ///     Checks whether the string representation of <paramref name="value" /> is contained in
    ///     <paramref name="collection" />.
    /// </summary>
    /// <remarks>
    ///     This method is used by <see cref="ArangoDbEnumerableExtensions" />. Therefore the order of input arguments is
    ///     important.
    ///     The filter arguments are only valid for <paramref name="value" />s that are sequences, lists, etc.
    /// </remarks>
    internal static bool ContainsValueOf(
        object value,
        IEnumerable<string> collection,
        FilterOperator @operator,
        bool allMustBeContained,
        IVirtualPropertyResolver resolver)
    {
        return ContainsValueOf(value, collection, @operator, allMustBeContained);
    }

    /// <summary>
    ///     Checks whether the string representation of <paramref name="value" /> is contained in
    ///     <paramref name="collection" />.
    /// </summary>
    /// <remarks>
    ///     This method is used by <see cref="ArangoDbEnumerableExtensions" />. Therefore the order of input arguments is
    ///     important.
    ///     The filter arguments are only valid for <paramref name="value" />s that are sequences, lists, etc.
    /// </remarks>
    internal static bool ContainsValueOf(
        object value,
        IEnumerable<string> collection,
        FilterOperator @operator,
        bool allMustBeContained,
        string filterPropertyName = null,
        object filterPropertyValue = null)
    {
        if (value == null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        if (collection == null)
        {
            throw new ArgumentNullException(nameof(collection));
        }

        List<string> data = collection as List<string> ?? collection.ToList();

        StringComparer comparer = StringComparer.OrdinalIgnoreCase;

        switch (value)
        {
            case string s:
                if (string.IsNullOrWhiteSpace(s))
                {
                    throw new ArgumentException(
                        "Value must be a valid string. It should not be an empty string or just whitespace.",
                        nameof(value));
                }

                return data.Contains(s, comparer);
            case double d:
                return data.Contains(d.ToString(CultureInfo.InvariantCulture), comparer);
            case int i:
                return data.Contains(i.ToString(CultureInfo.InvariantCulture), comparer);
            case long l:
                return data.Contains(l.ToString(CultureInfo.InvariantCulture), comparer);
            case float f:
                return data.Contains(f.ToString(CultureInfo.InvariantCulture), comparer);
            case DateTime dt:
                return data.Contains(dt.ToUniversalTime().ToString("O"), comparer);
            default:
                return data.Contains(value.ToString(), comparer);
        }
    }
}
