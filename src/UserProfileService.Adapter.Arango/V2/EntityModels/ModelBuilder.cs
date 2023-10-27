using System;
using System.Linq.Expressions;

namespace UserProfileService.Adapter.Arango.V2.EntityModels;

internal class ModelBuilder : IModelBuilder
{
    internal static IModelBuilder NewOne => new ModelBuilder();
    public ModelBuilderOptions Options { get; } = new ModelBuilderOptions();

    private ModelBuilder()
    {
    }

    public string GetCollectionName<TEntity>()
    {
        return Options.GetCollectionName<TEntity>();
    }

    public string GetCollectionName(Type type)
    {
        return Options.GetCollectionName(type);
    }

    /// <inheritdoc />
    public ModelBuilderOptions BuildOptions(string collectionPrefix, string queryCollectionPrefix)
    {
        return Options.Build(collectionPrefix, queryCollectionPrefix ?? collectionPrefix);
    }

    /// <inheritdoc />
    public IModelBuilderEntityOptions<TEntity> Entity<TEntity>()
    {
        return ModelBuilder<TEntity>.Create(this, Options);
    }
}

internal class ModelBuilder<TEntity> : IModelBuilderEntityOptions<TEntity>, IModelBuilderEntityQueryOptions
{
    public ModelBuilderOptions Options { get; }

    public ModelBuilder Parent { get; }

    private ModelBuilder(ModelBuilder parent, ModelBuilderOptions options)
    {
        Parent = parent;
        Options = options;
    }

    internal static IModelBuilderEntityOptions<TEntity> Create(
        ModelBuilder parent,
        ModelBuilderOptions options)
    {
        return new ModelBuilder<TEntity>(parent, options);
    }

    public IModelBuilderEntityQueryOptions NoCollection()
    {
        return this;
    }

    /// <inheritdoc />
    public IModelBuilderEntityQueryOptions Collection(string collectionName)
    {
        Options.AddTypeInformation<TEntity>(o => o.SetDefaultCollection(collectionName));

        return this;
    }

    public IModelBuilderEntityQueryOptions Collection<TAliasEntity>(string collectionName)
    {
        Options.AddTypeInformation<TEntity>(o => o.AddAliasCollectionDefinition<TAliasEntity>(collectionName));

        return this;
    }

    public IModelBuilderEntityQueryOptions Collection<TAliasEntityOne, TAliasEntityTwo>(string collectionName)
    {
        Options.AddTypeInformation<TEntity>(o => o.AddAliasCollectionDefinition<TAliasEntityOne>(collectionName));
        Options.AddTypeInformation<TEntity>(o => o.AddAliasCollectionDefinition<TAliasEntityTwo>(collectionName));

        return this;
    }

    public IModelBuilderEntityQueryOptions Collection<TAliasEntityOne, TAliasEntityTwo, TAliasEntityThree>(
        string collectionName)
    {
        Options.AddTypeInformation<TEntity>(o => o.AddAliasCollectionDefinition<TAliasEntityOne>(collectionName));
        Options.AddTypeInformation<TEntity>(o => o.AddAliasCollectionDefinition<TAliasEntityTwo>(collectionName));
        Options.AddTypeInformation<TEntity>(o => o.AddAliasCollectionDefinition<TAliasEntityThree>(collectionName));

        return this;
    }

    /// <inheritdoc />
    public void HasKeyIdentifier(string propertyName)
    {
        Options.AddTypeInformation<TEntity>(o => o.KeyPropertyName = propertyName);
    }

    /// <inheritdoc />
    public IModelBuilderEntityOptions<TEntity> HasTypeIdentification(
        string propertyName)
    {
        return HasTypeIdentification(propertyName, null);
    }

    /// <inheritdoc />
    public IModelBuilderEntityOptions<TEntity> HasTypeIdentification(
        string propertyName,
        object typeValue)
    {
        Options.AddTypeInformation<TEntity>(
            o
                => o.AddTypeInformation(propertyName, typeValue ?? typeof(TEntity).Name));

        return this;
    }

    public IModelBuilderEntityOptions<TEntity> HasAlias<TProp>(
        Expression<Func<TEntity, TProp>> typePropertySelector,
        object discriminatorTypeValue,
        params Type[] types)
    {
        if (typePropertySelector?.Body is not MemberExpression memberExpression)
        {
            throw new ArgumentException("Wrong expression type!", nameof(typePropertySelector));
        }

        if (string.IsNullOrEmpty(memberExpression.Member.Name))
        {
            throw new ArgumentException(
                "Cannot extract member name from expression.",
                nameof(typePropertySelector));
        }

        return HasAlias(memberExpression.Member.Name, discriminatorTypeValue, types);
    }

    public IModelBuilderEntityOptions<TEntity> HasAlias(
        string typePropertyName,
        object discriminatorTypeValue,
        params Type[] types)
    {
        HasAlias(types);

        Options.AddTypeInformation<TEntity>(
            o => o.AddAliasTypeInformation(
                typePropertyName,
                types,
                discriminatorTypeValue));

        return this;
    }

    public IModelBuilderEntityOptions<TEntity> HasAlias<TAlias>()
    {
        return HasAlias(typeof(TAlias));
    }

    public IModelBuilderEntityOptions<TEntity> HasAlias(params Type[] types)
    {
        if (types == null)
        {
            throw new ArgumentNullException(nameof(types));
        }

        if (types.Length == 0)
        {
            throw new ArgumentException("Value cannot be an empty collection.", nameof(types));
        }

        Options.AddAliasTypes<TEntity>(types);

        return this;
    }

    /// <inheritdoc />
    public IModelBuilderEntityOptions<TEntity> HasTypeIdentification<TProp>(
        Expression<Func<TEntity, TProp>> propertySelector)
    {
        return HasTypeIdentification(propertySelector, null);
    }

    /// <inheritdoc />
    public IModelBuilderEntityOptions<TEntity> HasTypeIdentification<TProp>(
        Expression<Func<TEntity, TProp>> propertySelector,
        object typeValue)
    {
        if (propertySelector?.Body is not MemberExpression memberExpression)
        {
            throw new ArgumentException("Wrong expression type!", nameof(propertySelector));
        }

        if (string.IsNullOrEmpty(memberExpression.Member.Name))
        {
            throw new ArgumentException("Cannot extract member name from expression.", nameof(propertySelector));
        }

        Options.AddTypeInformation<TEntity>(
            o
                => o.AddTypeInformation(memberExpression.Member.Name, typeValue ?? typeof(TEntity).Name));

        return this;
    }

    /// <inheritdoc />
    public IModelBuilderEntityOptions<TEntity> HasKeyIdentifier<TProp>(
        Expression<Func<TEntity, TProp>> propertySelector)
    {
        if (propertySelector?.Body is not MemberExpression memberExpression)
        {
            throw new ArgumentException("Wrong expression type!", nameof(propertySelector));
        }

        if (string.IsNullOrEmpty(memberExpression.Member.Name))
        {
            throw new ArgumentException("Cannot extract member name from expression.", nameof(propertySelector));
        }

        Options.AddTypeInformation<TEntity>(o => o.KeyPropertyName = memberExpression.Member.Name);

        return this;
    }

    public IModelBuilderEntityOptions<TEntity> AddChildRelation<TChild>()
    {
        return AddChildRelation<TChild>(_ => { });
    }

    public IModelBuilderEntityOptions<TEntity> AddChildRelation<TChild>(
        Action<IRelationBuilder<TEntity, TChild>> relationConfig)
    {
        var relationBuilder = new ModelBuilderRelationBuilder<TEntity, TChild>();

        relationConfig.Invoke(relationBuilder);

        Options.AddTypeInformation<TEntity>(o => o.AddRelation(relationBuilder));

        return this;
    }

    public IModelBuilderEntityOptions<TEntity> AddParentRelation<TParent>(
        Action<IRelationBuilder<TParent, TEntity>> relationConfig)
    {
        var relationBuilder = new ModelBuilderRelationBuilder<TParent, TEntity>();

        relationConfig.Invoke(relationBuilder);

        Options.AddTypeInformation<TEntity>(o => o.AddRelation(relationBuilder));

        return this;
    }

    public void QueryCollection(string collectionName)
    {
        if (Options.GetCollectionName<TEntity>() == collectionName)
        {
            throw new ArgumentException("Names of collection and query collection should not be the same!");
        }

        Options.TryGetAliasTypes(typeof(TEntity), out _);

        Options.AddTypeInformation<TEntity>(o => o.QueryCollectionName = collectionName);
    }
}
