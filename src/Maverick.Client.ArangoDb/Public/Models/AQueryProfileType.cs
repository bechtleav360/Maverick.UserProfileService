namespace Maverick.Client.ArangoDb.Public.Models;

/// <summary>
///     Controls the additional information of the query, that will be returned in the resulting object.
/// </summary>
public enum AQueryProfileType
{
    /// <summary>
    ///     Additional query profiling information will be returned
    ///     in the sub-attribute profile of the extra return attribute, if the query result
    ///     is not served from the query cache.
    /// </summary>
    Basic = 1,

    /// <summary>
    ///     the query will include execution stats per query plan node in sub-attribute stats.nodes of the extra return
    ///     attribute.
    ///     Additionally the query plan is returned in the sub-attribute extra.plan.
    /// </summary>
    Detailed = 2
}
