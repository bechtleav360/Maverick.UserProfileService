using System;
using System.Collections.Generic;
using System.Linq;
using UserProfileService.Adapter.Arango.V2.Contracts;
using UserProfileService.Adapter.Arango.V2.Extensions;

namespace UserProfileService.Adapter.Arango.V2.EntityModels;

internal class ModelBuilderOptionsType : IModelBuilderSubclass
{
    private string _collectionPrefix;
    private readonly Dictionary<Type, string> _collections = new Dictionary<Type, string>();

    private string _defaultCollectionName;

    private readonly Dictionary<int, List<ModelBuilderOptionsTypeRelation>> _indexedFromRelations =
        new Dictionary<int, List<ModelBuilderOptionsTypeRelation>>();

    private readonly Dictionary<int, ModelBuilderOptionsTypeRelation> _indexedFromToRelations =
        new Dictionary<int, ModelBuilderOptionsTypeRelation>();

    private readonly Dictionary<int, List<ModelBuilderOptionsTypeRelation>> _indexedToRelations =
        new Dictionary<int, List<ModelBuilderOptionsTypeRelation>>();

    private string _queryCollectionName;
    private string _queryCollectionPrefix;

    internal string KeyPropertyName { get; set; }

    internal Type MainType { get; }

    internal Dictionary<Type, List<TypePropertyType>> PropertyMapperConfiguration { get; } =
        new Dictionary<Type, List<TypePropertyType>>();

    internal string QueryCollectionName
    {
        get =>
            _queryCollectionName?.GetPrefixedCollectionName(
                _queryCollectionPrefix
                ?? WellKnownDatabaseKeys
                    .CollectionPrefixUserProfileService);
        set => _queryCollectionName = value;
    }

    internal List<ModelBuilderOptionsTypeRelation> Relations { get; } = new List<ModelBuilderOptionsTypeRelation>();

    // Keys: Related type (either main type or alias types).
    // Values: Name of the type property / constant value that will identify the type.
    internal Dictionary<Type, Tuple<string, object>> TypeInformation { get; } =
        new Dictionary<Type, Tuple<string, object>>();

    /// <inheritdoc />
    public List<IModelBuilderSubclass> Children { get; } = new List<IModelBuilderSubclass>();

    /// <inheritdoc />
    public ModelBuilderOptions Options { get; }

    /// <inheritdoc />
    public IModelBuilderSubclass Parent { get; }

    internal ModelBuilderOptionsType(
        ModelBuilderOptions options,
        Type mainType,
        IModelBuilderSubclass parent = null)
    {
        Parent = parent;
        Options = options;
        MainType = mainType;
    }

    private void SetTypeInformation(
        Type concerningType,
        string propertyName,
        object matchingValue)
    {
        if (TypeInformation.ContainsKey(concerningType))
        {
            TypeInformation[concerningType] = new Tuple<string, object>(propertyName, matchingValue);

            return;
        }

        TypeInformation.Add(concerningType, new Tuple<string, object>(propertyName, matchingValue));
    }

    private static void AddFromToRelation(
        ModelBuilderOptionsTypeRelation obj,
        Func<ModelBuilderOptionsTypeRelation, Type> typeSelector,
        Dictionary<int, List<ModelBuilderOptionsTypeRelation>> toBeUpdated)
    {
        if (!toBeUpdated.ContainsKey(typeSelector.Invoke(obj).GetHashCode()))
        {
            toBeUpdated.Add(
                typeSelector.Invoke(obj).GetHashCode(),
                new List<ModelBuilderOptionsTypeRelation>
                {
                    obj
                });

            return;
        }

        if (toBeUpdated[typeSelector.Invoke(obj).GetHashCode()].Contains(obj))
        {
            return;
        }

        toBeUpdated[typeSelector.Invoke(obj).GetHashCode()].Add(obj);
    }

    internal IEnumerable<string> GetCollectionNames()
    {
        return _collections
            .Values
            .Append(_defaultCollectionName)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(
                name => name.GetPrefixedCollectionName(
                    _collectionPrefix
                    ?? WellKnownDatabaseKeys
                        .CollectionPrefixUserProfileService));
    }

    internal string GetCollectionName(Type alias)
    {
        if (alias == null)
        {
            throw new ArgumentNullException(nameof(alias));
        }

        // ReSharper disable once CanSimplifyDictionaryLookupWithTryGetValue
        string name = _collections.ContainsKey(alias)
            ? _collections[alias]
            : _defaultCollectionName;

        return name?.GetPrefixedCollectionName(
            _collectionPrefix ?? WellKnownDatabaseKeys.CollectionPrefixUserProfileService);
    }

    internal void AddAliasCollectionDefinition<TAliasType>(string name)
    {
        if (name == null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Collection name cannot be null or whitespace.", nameof(name));
        }

        _collections.TryAdd(typeof(TAliasType), name);
    }

    // is used to set a default collection name (including all sub types /alias types).
    internal void SetDefaultCollection(string name)
    {
        if (name == null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Collection name cannot be null or whitespace.", nameof(name));
        }

        _collections.TryAdd(MainType, name);
        _defaultCollectionName = name;
    }

    internal void AddTypeInformation(string propertyName, object matchingValue)
    {
        SetTypeInformation(MainType, propertyName, matchingValue);
    }

    internal void AddAliasTypeInformation(
        string propertyName,
        IList<Type> aliasTypes,
        object matchingValue)
    {
        foreach (Type aliasType in aliasTypes)
        {
            SetTypeInformation(aliasType, propertyName, matchingValue);
        }
    }

    /// <summary>
    ///     Gets the type information either of main type (leave <paramref name="concerningType" /> null) or one alias type.
    ///     <br />
    ///     if no type information could be found for <paramref name="concerningType" /> (if not null), the one of
    ///     <see cref="MainType" /> will be returned.
    /// </summary>
    /// <param name="concerningType">
    ///     The type whose inforation to be returned. If <c>null</c>, the information about the
    ///     <see cref="MainType" /> will be returned.
    /// </param>
    /// <returns>A tuple of property name and value that represents the type information.</returns>
    internal (string propertyName, object propertyValue) GetTypeInformation(Type concerningType = null)
    {
        // fallback to main type, if nothing was configured for concerning type
        bool success =
            (concerningType != null
                && TypeInformation.TryGetValue(concerningType, out Tuple<string, object> result))
            || TypeInformation.TryGetValue(MainType, out result);

        if (!success)
        {
            return (null, null);
        }

        (string propertyName, object propertyValue) = result;

        return (propertyName, propertyValue);
    }

    internal void AddRelation<TFrom, TTo>(IRelationBuilder<TFrom, TTo> options)
    {
        ModelBuilderOptionsTypeRelation newObj = options.Build(this);

        if (Relations.Contains(newObj))
        {
            return;
        }

        Relations.Add(newObj);
        _indexedFromToRelations.Add(HashCode.Combine(newObj.FromType, newObj.ToType), newObj);
        AddFromToRelation(newObj, x => x.FromType, _indexedFromRelations);
        AddFromToRelation(newObj, x => x.ToType, _indexedToRelations);
    }

    internal ModelBuilderOptionsTypeRelation GetRelation<TFrom, TTo>()
    {
        return GetRelation(typeof(TFrom), typeof(TTo));
    }

    internal ModelBuilderOptionsTypeRelation GetRelation(Type fromType, Type toType)
    {
        return fromType != null
            && toType != null
            && _indexedFromToRelations.TryGetValue(
                HashCode.Combine(fromType, toType),
                out ModelBuilderOptionsTypeRelation relation)
                ? relation
                : null;
    }

    internal IEnumerable<TElem> GetFromRelations<TElem>(
        Type fromType,
        Func<ModelBuilderOptionsTypeRelation, TElem> elementSelector)
    {
        return fromType != null
            && _indexedFromRelations.TryGetValue(
                fromType.GetHashCode(),
                out List<ModelBuilderOptionsTypeRelation> relation)
                ? relation.Select(elementSelector)
                : Enumerable.Empty<TElem>();
    }

    internal IEnumerable<TElem> GetToRelations<TElem>(
        Type fromType,
        Func<ModelBuilderOptionsTypeRelation, TElem> elementSelector)
    {
        return fromType != null
            && _indexedToRelations.TryGetValue(
                fromType.GetHashCode(),
                out List<ModelBuilderOptionsTypeRelation> relation)
                ? relation.Select(elementSelector)
                : Enumerable.Empty<TElem>();
    }

    /// <inheritdoc />
    public void Build(string collectionPrefix, string queryCollectionPrefix)
    {
        _collectionPrefix = collectionPrefix ?? WellKnownDatabaseKeys.CollectionPrefixUserProfileService;
        _queryCollectionPrefix = queryCollectionPrefix ?? collectionPrefix;

        foreach (IModelBuilderSubclass child in Children)
        {
            child.Build(collectionPrefix, queryCollectionPrefix);
        }

        foreach (ModelBuilderOptionsTypeRelation child in Relations)
        {
            child.Build(collectionPrefix, queryCollectionPrefix);
        }
    }
}
