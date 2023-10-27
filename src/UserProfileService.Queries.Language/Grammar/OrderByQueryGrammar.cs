using Sprache;
using UserProfileService.Queries.Language.ExpressionOperators;
using UserProfileService.Queries.Language.Models;

namespace UserProfileService.Queries.Language.Grammar;

/// <summary>
///     The oderBy query grammar is used to parse a oderBy-Operation string and return
///     a list of <see cref="SortedProperty" /> that can be used to order the result in a proper way.
/// </summary>
public sealed class OrderByQueryGrammar
{
    /// <summary>
    ///     The end of the query or the next property that will be parsed.
    /// </summary>
    internal Parser<string> EndOfProperty =>
        Parse.IgnoreCase(",").Or(Parse.LineTerminator.End()).Text().Named("EndOfProperty(',') or EndOfQuery");

    /// <summary>
    ///     Get the property for which the sorting should be made.
    /// </summary>
    internal Parser<string> Property => Parse.Regex("[A-Za-z]*").Text().Named("OderByValueName");

    /// <summary>
    ///     The whole sorted object that will be parsed.
    ///     It consists out of a property and a sort oder.
    /// </summary>
    internal Parser<SortedProperty> SortedPropertyParser =>
        from sortProperty in Property.Token()
        from sortDirection in SortOrder.Token()
        from separator in EndOfProperty.Token()
        select new SortedProperty(sortProperty, sortDirection);

    /// <summary>
    ///     Gets the sort of the property. The sort order
    ///     can be descending or ascending. If the sort order is missing
    ///     the default value is ascending.
    /// </summary>
    internal Parser<SortDirection> SortOrder =>
        Parse.IgnoreCase("asc")
            .Return(SortDirection.Ascending)
            .Or(Parse.IgnoreCase("desc").Return(SortDirection.Descending))
            .Or(Parse.Return(SortDirection.Ascending))
            .Named("SorteOrder ASC|DESC");

    /// <summary>
    ///     Parses the query and generate at least a list with one
    ///     <see cref="SortedProperty" /> item.
    /// </summary>
    public Parser<List<SortedProperty>> OrderByQuery =>
        from sorted in SortedPropertyParser.AtLeastOnce().Named("SortedProperty")
        select sorted.ToList();
}
