using System;

namespace UserProfileService.Adapter.Arango.V2.EntityModels;

/// <summary>
///     Represents a model builder for entity models.
/// </summary>
public interface IModelBuilder
{
    /// <summary>
    ///     Gets an entity options builder for the specified entity type.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <returns>An entity options builder.</returns>
    IModelBuilderEntityOptions<TEntity> Entity<TEntity>();

    /// <summary>
    ///     Gets the collection name for the specified entity type.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <returns>The collection name.</returns>
    string GetCollectionName<TEntity>();

    /// <summary>
    ///     Gets the collection name for the specified entity type.
    /// </summary>
    /// <param name="type">The type to get the collection name for.</param>
    /// <returns>The collection name.</returns>
    string GetCollectionName(Type type);

    /// <summary>
    ///     Builds the model builder options using the specified collection prefixes.
    /// </summary>
    /// <param name="collectionPrefix">The collection prefix for building.</param>
    /// <param name="queryCollectionPrefix">The collection prefix for queries (optional).</param>
    /// <returns>The model builder options.</returns>
    ModelBuilderOptions BuildOptions(string collectionPrefix, string queryCollectionPrefix = null);
}
