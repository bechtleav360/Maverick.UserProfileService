namespace Maverick.Client.ArangoDb.Public.QueryBuilder;

/// <summary>
///     abstract containing some properties to build an AQL query
/// </summary>
public abstract class Query
{
    /// <summary>
    ///     The name of a collection contains in the query
    /// </summary>
    public string CollectionName { get; set; }

    /// <summary>
    ///     Should the query uses filtering options?
    /// </summary>
    public bool EnableFiltering { get; set; }

    /// <summary>
    ///     Does the query works on a single document?
    /// </summary>
    public bool OnSingleDocument { get; set; } = true;

    /// <summary>
    ///     Initialize a query with a collection name
    /// </summary>
    /// <param name="collectionName"></param>
    public Query(string collectionName)
    {
        CollectionName = collectionName;
    }

    /// <summary>
    ///     Abstract method to build a request
    /// </summary>
    /// <returns></returns>
    public abstract string BuildRequest();
}
