using UserProfileService.Queries.Language.Models;
using UserProfileService.Queries.Language.TreeDefinition;

namespace UserProfileService.Adapter.Marten.Abstractions;

/// <summary>
///     The query converter is used to parse out of the
///     query string either a tree or a list that can be used to
///     build a query for the database.
/// </summary>
public interface IQueryConverter
{
    /// <summary>
    ///     Creates out of a filter query a <see cref="TreeNode" /> that can be used
    ///     to create a intelligible query for a database.
    /// </summary>
    /// <param name="filterQuery">The filter query that contains the query for the database.</param>
    /// <returns>A <see cref="TreeNode" /> that can be traversed to create a intelligible query for a database. </returns>
    TreeNode? CreateFilterQueryTree(string filterQuery);

    /// <summary>
    ///     Creates out of a order by query a list of  <see cref="SortedProperty" /> that can be used
    ///     to create a intelligible query for a database.
    /// </summary>
    /// <param name="orderByQuery">The order by query that contains the query for the database.</param>
    /// <returns>A list of <see cref="SortedProperty" />  that can be used to create a intelligible query for a database.</returns>
    List<SortedProperty>? CreateOrderByQuery(string orderByQuery);
}
