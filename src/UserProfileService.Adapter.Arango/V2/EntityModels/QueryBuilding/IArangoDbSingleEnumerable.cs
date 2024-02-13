namespace UserProfileService.Adapter.Arango.V2.EntityModels.QueryBuilding;

/// <summary>
///     Represents an enumerable for querying a single entity in ArangoDB.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
public interface IArangoDbSingleEnumerable<TEntity>
{
    /// <summary>
    ///     Gets a typed enumerable for the specified entity type.
    /// </summary>
    /// <returns>An enumerable of the specified entity type.</returns>
    ArangoDbEnumerable<TEntity> GetTypedEnumerable();
}
