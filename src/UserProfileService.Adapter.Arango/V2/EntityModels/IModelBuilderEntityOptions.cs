using System;
using System.Linq.Expressions;

namespace UserProfileService.Adapter.Arango.V2.EntityModels;

public interface IModelBuilderEntityOptions<TEntity> : IModelBuilderEntityOptions
{
    IModelBuilderEntityOptions<TEntity> HasKeyIdentifier<TProp>(Expression<Func<TEntity, TProp>> propertySelector);
    IModelBuilderEntityOptions<TEntity> AddChildRelation<TChild>();

    IModelBuilderEntityOptions<TEntity> AddChildRelation<TChild>(
        Action<IRelationBuilder<TEntity, TChild>> relationConfig);

    IModelBuilderEntityOptions<TEntity> AddParentRelation<TParent>(
        Action<IRelationBuilder<TParent, TEntity>> relationConfig);

    IModelBuilderEntityOptions<TEntity> HasTypeIdentification<TProp>(
        Expression<Func<TEntity, TProp>> propertySelector,
        object typeValue);

    IModelBuilderEntityOptions<TEntity> HasTypeIdentification<TProp>(
        Expression<Func<TEntity, TProp>> propertySelector);

    IModelBuilderEntityOptions<TEntity> HasTypeIdentification(
        string propertyName);

    IModelBuilderEntityOptions<TEntity> HasTypeIdentification(
        string propertyName,
        object typeValue);

    IModelBuilderEntityOptions<TEntity> HasAlias<TProp>(
        Expression<Func<TEntity, TProp>> typePropertySelector,
        object discriminatorTypeValue,
        params Type[] types);

    IModelBuilderEntityOptions<TEntity> HasAlias(params Type[] types);
    
    IModelBuilderEntityOptions<TEntity> HasAlias<TAlias>();
}

public interface IModelBuilderEntityOptions
{
    IModelBuilderEntityQueryOptions NoCollection();
    IModelBuilderEntityQueryOptions Collection(string collectionName);
    IModelBuilderEntityQueryOptions Collection<TAliasEntity>(string collectionName);
    IModelBuilderEntityQueryOptions Collection<TAliasEntityOne, TAliasEntityTwo>(string collectionName);

    IModelBuilderEntityQueryOptions Collection<TAliasEntityOne, TAliasEntityTwo, TAliasEntityThree>(
        string collectionName);

    void HasKeyIdentifier(string propertyName);
}
