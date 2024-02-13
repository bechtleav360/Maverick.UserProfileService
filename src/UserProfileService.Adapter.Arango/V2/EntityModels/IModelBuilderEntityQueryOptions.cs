namespace UserProfileService.Adapter.Arango.V2.EntityModels;

/// <summary>
///     Represents options for querying entities in a model builder.
/// </summary>
public interface IModelBuilderEntityQueryOptions
{
    /// <summary>
    ///     Specifies a collection name for the <typeparamref name="TAliasEntity"/> alias entity type.
    /// </summary>
    /// <typeparam name="TAliasEntity">The type of the alias entity.</typeparam>
    /// <param name="collectionName">The name of the collection.</param>
    /// <returns>The updated query options.</returns>
    IModelBuilderEntityQueryOptions Collection<TAliasEntity>(string collectionName);

    /// <summary>
    ///     Specifies a collection name for the <typeparamref name="TAliasEntityOne"/>
    ///     and <typeparamref name="TAliasEntityTwo"/> alias entity types.
    /// </summary>
    /// <typeparam name="TAliasEntityOne">The type of the first alias entity.</typeparam>
    /// <typeparam name="TAliasEntityTwo">The type of the second alias entity.</typeparam>
    /// <param name="collectionName">The name of the collection.</param>
    /// <returns>The updated query options.</returns>
    IModelBuilderEntityQueryOptions Collection<TAliasEntityOne, TAliasEntityTwo>(string collectionName);

    /// <summary>
    ///     Specifies a collection name for three alias entity types.
    /// </summary>
    /// <typeparam name="TAliasEntityOne">The type of the first alias entity.</typeparam>
    /// <typeparam name="TAliasEntityTwo">The type of the second alias entity.</typeparam>
    /// <typeparam name="TAliasEntityThree">The type of the third alias entity.</typeparam>
    /// <param name="collectionName">The name of the collection.</param>
    /// <returns>The updated query options.</returns>
    IModelBuilderEntityQueryOptions Collection<TAliasEntityOne, TAliasEntityTwo, TAliasEntityThree>(
        string collectionName);

    /// <summary>
    ///     Specifies the name of the query collection.
    /// </summary>
    /// <param name="collectionName">
    ///     The name of the query collection.
    /// </param>
    void QueryCollection(string collectionName);
}
