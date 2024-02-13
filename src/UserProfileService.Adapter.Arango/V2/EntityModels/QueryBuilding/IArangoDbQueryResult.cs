namespace UserProfileService.Adapter.Arango.V2.EntityModels.QueryBuilding;

/// <summary>
///     Represents an interface for retrieving the compiled query strings for a query against ArangoDB.
/// </summary>
public interface IArangoDbQueryResult
{
    /// <summary>
    ///     Gets the query string associated with the result.
    /// </summary>
    /// <returns>The query string.</returns>
    string GetQueryString();
    /// <summary>
    ///     Gets the count query string associated with the result.
    /// </summary>
    /// <returns>The count query string.</returns>
    string GetCountQueryString();
}
