using System;
using UserProfileService.Adapter.Arango.V2.Contracts;

namespace UserProfileService.Adapter.Arango.V2.EntityModels.QueryBuilding;

/// <summary>
///     Represents an interface for querying entities in ArangoDB.
/// </summary>
public interface IArangoDbEnumerable
{
    /// <summary>
    ///     Gets an enumerable of entities.
    /// </summary>
    /// <returns>An enumerable of entities.</returns>
    ArangoDbEnumerable GetEnumerable();
    /// <summary>
    ///     Compiles a query result for the specified entity.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="scope">The collection scope.</param>
    /// <returns>The compiled query result.</returns>
    IArangoDbQueryResult Compile<TEntity>(CollectionScope scope);
    /// <summary>
    ///     Gets the inner type associated with the entity.
    /// </summary>
    /// <returns>The inner type.</returns>
    Type GetInnerType();
}

/// <summary>
///     Represents an interface for querying typed entities in ArangoDB.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
public interface IArangoDbEnumerable<TEntity> : IArangoDbEnumerable
{
    /// <summary>
    ///     Gets a typed enumerable for the specified entity type <typeparamref name="TEntity"/>.
    /// </summary>
    /// <returns>An enumerable of the specified entity type.</returns>
    ArangoDbEnumerable<TEntity> GetTypedEnumerable();
    /// <summary>
    ///     Compiles a query result for the specified entity type <typeparamref name="TEntity"/>.
    /// </summary>
    /// <param name="scope">The collection scope.</param>
    /// <returns>The compiled query result.</returns>
    IArangoDbQueryResult Compile(CollectionScope scope);
}
