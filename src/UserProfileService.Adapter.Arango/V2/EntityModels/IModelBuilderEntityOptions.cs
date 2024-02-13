using System;
using System.Linq.Expressions;

namespace UserProfileService.Adapter.Arango.V2.EntityModels;

/// <summary>
///     Entity options for <see cref="IModelBuilder"/> with a fluent-style interface.
/// </summary>
/// <typeparam name="TEntity">The type of the entity the model is configured for.</typeparam>
public interface IModelBuilderEntityOptions<TEntity> : IModelBuilderEntityOptions
{
    /// <summary>
    ///     Specifies the key identifier property for the entity.
    /// </summary>
    /// <typeparam name="TProp">The type of the key identifier property.</typeparam>
    /// <param name="propertySelector">Expression representing the key identifier property.</param>
    /// <returns>The updated entity options.</returns>
    IModelBuilderEntityOptions<TEntity> HasKeyIdentifier<TProp>(Expression<Func<TEntity, TProp>> propertySelector);

    /// <summary>
    ///     Adds a child relation to the entity for child entities of type <typeparamref name="TChild"></typeparamref>.
    /// </summary>
    /// <typeparam name="TChild">The type of the child entity.</typeparam>
    /// <returns>The updated entity options.</returns>
    IModelBuilderEntityOptions<TEntity> AddChildRelation<TChild>();

    /// <summary>
    ///     Adds a child relation to the entity for child entities of type
    ///     <typeparamref name="TChild"></typeparamref> with custom configuration.
    /// </summary>
    /// <typeparam name="TChild">The type of the child entity.</typeparam>
    /// <param name="relationConfig">Action to configure the relation builder.</param>
    /// <returns>The updated entity options.</returns>
    IModelBuilderEntityOptions<TEntity> AddChildRelation<TChild>(
        Action<IRelationBuilder<TEntity, TChild>> relationConfig);

    /// <summary>
    ///     Adds a parent relation to the entity for parent entites of
    ///     type <typeparamref name="TParent"/> with custom configuration.
    /// </summary>
    /// <typeparam name="TParent">The type of the parent entity.</typeparam>
    /// <param name="relationConfig">Action to configure the relation builder.</param>
    /// <returns>The updated entity options.</returns>
    IModelBuilderEntityOptions<TEntity> AddParentRelation<TParent>(
        Action<IRelationBuilder<TParent, TEntity>> relationConfig);

    /// <summary>
    ///     Specifies type identification for the entity based on a property value.
    /// </summary>
    /// <typeparam name="TProp">The type of the property used for type identification.</typeparam>
    /// <param name="propertySelector">Expression representing the property.</param>
    /// <param name="typeValue">The value representing the entity type.</param>
    /// <returns>The updated entity options.</returns>
    IModelBuilderEntityOptions<TEntity> HasTypeIdentification<TProp>(
        Expression<Func<TEntity, TProp>> propertySelector,
        object typeValue);

    /// <summary>
    ///     Specifies type identification for the entity based on a property value.
    /// </summary>
    /// <typeparam name="TProp">The type of the property used for type identification.</typeparam>
    /// <param name="propertySelector">Expression representing the property.</param>
    /// <returns>The updated entity options.</returns>
    IModelBuilderEntityOptions<TEntity> HasTypeIdentification<TProp>(
        Expression<Func<TEntity, TProp>> propertySelector);

    /// <summary>
    ///     Specifies type identification for the entity based on a property name.
    /// </summary>
    /// <param name="propertyName">The name of the property used for type identification.</param>
    /// <returns>The updated entity options.</returns>
    IModelBuilderEntityOptions<TEntity> HasTypeIdentification(
        string propertyName);

    /// <summary>
    ///     Specifies type identification for the entity based on a property name and value.
    /// </summary>
    /// <param name="propertyName">The name of the property used for type identification.</param>
    /// <param name="typeValue">The value representing the entity type.</param>
    /// <returns>The updated entity options.</returns>
    IModelBuilderEntityOptions<TEntity> HasTypeIdentification(
        string propertyName,
        object typeValue);

    /// <summary>
    ///     Specifies type aliases for the entity.
    /// </summary>
    /// <typeparam name="TProp">The type of the property used for aliasing.</typeparam>
    /// <param name="typePropertySelector">Expression representing the property.</param>
    /// <param name="discriminatorTypeValue">The value representing the discriminator type.</param>
    /// <param name="types">Additional types associated with the alias.</param>
    /// <returns>The updated entity options.</returns>
    IModelBuilderEntityOptions<TEntity> HasAlias<TProp>(
        Expression<Func<TEntity, TProp>> typePropertySelector,
        object discriminatorTypeValue,
        params Type[] types);

    /// <summary>
    ///     Specifies type aliases for the entity.
    /// </summary>
    /// <param name="types">The types associated with the alias.</param>
    /// <returns>The updated entity options.</returns>
    IModelBuilderEntityOptions<TEntity> HasAlias(params Type[] types);

    /// <summary>
    ///     Specifies a type alias for the entity.
    /// </summary>
    /// <typeparam name="TAlias">The type alias.</typeparam>
    /// <returns>The updated entity options.</returns>
    IModelBuilderEntityOptions<TEntity> HasAlias<TAlias>();
}

/// <summary>
///     Entity options for <see cref="IModelBuilder"/> with a fluent-style interface.
/// </summary>
public interface IModelBuilderEntityOptions
{
    /// <summary>
    ///     Specifies that no collection is associated with the entity.
    ///     In case only a query collection is required.
    /// </summary>
    /// <returns>The query options without a collection.</returns>
    IModelBuilderEntityQueryOptions NoCollection();

    /// <summary>
    ///     Specifies a collection by its name.
    /// </summary>
    /// <param name="collectionName">The name of the collection.</param>
    /// <returns>The query options for the specified collection.</returns>
    IModelBuilderEntityQueryOptions Collection(string collectionName);

    /// <summary>
    ///     Specifies a collection by its name for a specific alias entity type.
    /// </summary>
    /// <typeparam name="TAliasEntity">The type of the alias entity.</typeparam>
    /// <param name="collectionName">The name of the collection.</param>
    /// <returns>The query options for the specified collection.</returns>
    IModelBuilderEntityQueryOptions Collection<TAliasEntity>(string collectionName);

    /// <summary>
    ///     Specifies a collection by its name for two alias entity types.
    /// </summary>
    /// <typeparam name="TAliasEntityOne">The type of the first alias entity.</typeparam>
    /// <typeparam name="TAliasEntityTwo">The type of the second alias entity.</typeparam>
    /// <param name="collectionName">The name of the collection.</param>
    /// <returns>The query options for the specified collection.</returns>
    IModelBuilderEntityQueryOptions Collection<TAliasEntityOne, TAliasEntityTwo>(string collectionName);

    /// <summary>
    ///     Specifies a collection by its name for three alias entity types.
    /// </summary>
    /// <typeparam name="TAliasEntityOne">The type of the first alias entity.</typeparam>
    /// <typeparam name="TAliasEntityTwo">The type of the second alias entity.</typeparam>
    /// <typeparam name="TAliasEntityThree">The type of the third alias entity.</typeparam>
    /// <param name="collectionName">The name of the collection.</param>
    /// <returns>The query options for the specified collection.</returns>
    IModelBuilderEntityQueryOptions Collection<TAliasEntityOne, TAliasEntityTwo, TAliasEntityThree>(
        string collectionName);

    /// <summary>
    ///     Specifies the key identifier property for the entity.
    /// </summary>
    /// <param name="propertyName">The name of the key identifier property.</param>
    void HasKeyIdentifier(string propertyName);
}
