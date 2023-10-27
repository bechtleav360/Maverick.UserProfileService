using Microsoft.Extensions.Logging;
using Sprache;
using UserProfileService.Adapter.Marten.Abstractions;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Queries.Language.Grammar;
using UserProfileService.Queries.Language.Models;
using UserProfileService.Queries.Language.TreeDefinition;

namespace UserProfileService.Adapter.Marten.Implementations;

/// <summary>
///     The query converter is used to parse out of the
///     query string either a tree or a list of <see cref="SortedProperty" /> that can be used to
///     build a query for the database.
/// </summary>
public class QueryConverter : IQueryConverter
{
    private readonly ILogger<QueryConverter> _logger;

    /// <summary>
    ///     Creates a <see cref="QueryConverter" /> object.
    /// </summary>
    /// <param name="logger">The logger that is used to create messages with different severities.</param>
    public QueryConverter(ILogger<QueryConverter> logger)
    {
        _logger = logger;
    }

    /// <summary>
    ///     Creates out of a filter query a <see cref="TreeNode" /> that can be used
    ///     to create a intelligible query for a database.
    /// </summary>
    /// <param name="filterQuery">The filter query that contains the query for the database.</param>
    /// <returns>A <see cref="TreeNode" /> that can be traversed to create a intelligible query for a database. </returns>
    /// <exception cref="ArgumentNullException">
    ///     If the
    ///     <paramref name="filterQuery" />
    ///     is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     If the
    ///     <paramref name="filterQuery" />
    ///     is empty or containing whitespaces.
    /// </exception>
    public TreeNode? CreateFilterQueryTree(string? filterQuery)
    {
        _logger.EnterMethod();

        if (filterQuery == null)
        {
            throw new ArgumentNullException(nameof(filterQuery));
        }

        if (string.IsNullOrWhiteSpace(filterQuery))
        {
            throw new ArgumentException(nameof(filterQuery));
        }

        _logger.LogInfoMessage(
            "Trying build a tree out of the queryString '{queryString}'",
            LogHelpers.Arguments(filterQuery));

        var filterTreeParser = new FilterQueryGrammar(filterQuery);

        TreeNode? treeNode = filterTreeParser.QueryFilterParser.Parse(filterQuery);

        _logger.LogInfoMessage(
            "Building a tree out of the filter query '{queryString}' was successful.",
            LogHelpers.Arguments(filterQuery));

        return _logger.ExitMethod(treeNode);
    }

    /// <summary>
    ///     Creates out of a order by query a list of  <see cref="SortedProperty" /> that can be used
    ///     to create a intelligible query for a database.
    /// </summary>
    /// <param name="orderByQuery">The order by query that contains the query for the database.</param>
    /// <returns>A list of <see cref="SortedProperty" />  that can be used to create a intelligible query for a database.</returns>
    /// <exception cref="ArgumentNullException">
    ///     If the
    ///     <paramref name="orderByQuery" />
    ///     is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     If the
    ///     <paramref name="orderByQuery" />
    ///     is empty or containing whitespaces.
    /// </exception>
    public List<SortedProperty>? CreateOrderByQuery(string? orderByQuery)
    {
        _logger.EnterMethod();

        if (orderByQuery == null)
        {
            throw new ArgumentNullException(nameof(orderByQuery));
        }

        if (string.IsNullOrWhiteSpace(orderByQuery))
        {
            throw new ArgumentException(nameof(orderByQuery));
        }

        _logger.LogInfoMessage(
            "Trying to create a list out of the orderBy query {orderByQuery}.",
            orderByQuery.AsArgumentList());

        var orderByParser = new OrderByQueryGrammar();

        List<SortedProperty>? sortedList = orderByParser.OrderByQuery.Parse(orderByQuery);

        _logger.LogInfoMessage(
            "Building a tree out of the filter orderBy query '{queryString}' was successful.",
            orderByQuery.AsArgumentList());

        if (_logger.IsEnabledForDebug())
        {
            _logger.LogDebugMessage("The sorted list: '{sortedList}'.", LogHelpers.Arguments(sortedList.ToLogString()));
        }

        return _logger.ExitMethod(sortedList);
    }
}
