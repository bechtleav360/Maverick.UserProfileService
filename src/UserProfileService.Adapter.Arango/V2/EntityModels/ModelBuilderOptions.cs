using System;
using System.Collections.Generic;
using System.Linq;

namespace UserProfileService.Adapter.Arango.V2.EntityModels;

public class ModelBuilderOptions
{
    // keys: alias types, values reference types (i.e. interface)
    private Dictionary<Type, List<Type>> RegisteredAliasToMainTypes { get; } = new Dictionary<Type, List<Type>>();

    // keys reference types (i.e. interface), values: alias types
    private Dictionary<Type, List<Type>> RegisteredMainToAliasTypes { get; } = new Dictionary<Type, List<Type>>();

    private Dictionary<Type, ModelBuilderOptionsType> SavedInfos { get; } =
        new Dictionary<Type, ModelBuilderOptionsType>();

    private ModelBuilderOptionsType GetValue(Type type)
    {
        if (type == null)
        {
            return null;
        }

        if (SavedInfos.TryGetValue(type, out ModelBuilderOptionsType o))
        {
            return o;
        }

        if (!TryGetMainTypes(type, out IReadOnlyCollection<Type> aliasTypes))
        {
            return null;
        }

        return aliasTypes.Any(aliasType => SavedInfos.TryGetValue(aliasType, out o)) ? o : null;
    }

    private void AddEntryToMainToAliasTypeMapping(
        Type aliasType,
        Type mainType)
    {
        AddToTypeMapping(RegisteredMainToAliasTypes, mainType, aliasType);
    }

    private void AddEntryToAliasToMainTypeMapping(
        Type aliasType,
        Type mainType)
    {
        AddToTypeMapping(RegisteredAliasToMainTypes, aliasType, mainType);
    }

    private static void AddToTypeMapping(
        Dictionary<Type, List<Type>> tyeMapping,
        Type typeToAddAsKey,
        Type typeToAddAsValue)
    {
        if (!tyeMapping.ContainsKey(typeToAddAsKey))
        {
            tyeMapping.Add(
                typeToAddAsKey,
                new List<Type>
                {
                    typeToAddAsValue
                });

            return;
        }

        if (!tyeMapping[typeToAddAsKey].Contains(typeToAddAsValue))
        {
            tyeMapping[typeToAddAsKey].Add(typeToAddAsValue);
        }
    }

    internal ModelBuilderOptions Build(string collectionPrefix, string queryCollectionPrefix)
    {
        foreach (ModelBuilderOptionsType child in SavedInfos.Values)
        {
            child.Build(collectionPrefix, queryCollectionPrefix);
        }

        return this;
    }

    internal bool ContainsTypeDefinition<TEntity>()
    {
        return GetValue(typeof(TEntity)) != null;
    }

    internal void AddTypeInformation<T>(Action<ModelBuilderOptionsType> typeOption)
    {
        ModelBuilderOptionsType m;

        if (!SavedInfos.ContainsKey(typeof(T)))
        {
            m = new ModelBuilderOptionsType(this, typeof(T));
            SavedInfos.Add(typeof(T), m);
        }

        if (!SavedInfos.TryGetValue(typeof(T), out m))
        {
            m = new ModelBuilderOptionsType(this, typeof(T));
        }

        typeOption.Invoke(m);
    }

    internal void AddAliasTypes<TEntity>(params Type[] types)
    {
        if (types == null)
        {
            throw new ArgumentNullException(nameof(types));
        }

        if (types.Length == 0)
        {
            throw new ArgumentException("Value cannot be an empty collection.", nameof(types));
        }

        foreach (Type type in types)
        {
            if (type == null)
            {
                continue;
            }

            AddEntryToAliasToMainTypeMapping(type, typeof(TEntity));
            AddEntryToMainToAliasTypeMapping(type, typeof(TEntity));
        }
    }

    internal bool TryGetMainTypes(Type aliasType, out IReadOnlyCollection<Type> aliasTypes)
    {
        if (!RegisteredAliasToMainTypes.ContainsKey(aliasType))
        {
            aliasTypes = default;

            return false;
        }

        aliasTypes = RegisteredAliasToMainTypes[aliasType];

        return true;
    }

    internal bool TryGetAliasTypes(Type mainType, out IReadOnlyCollection<Type> aliasTypes)
    {
        // ReSharper disable once CanSimplifyDictionaryLookupWithTryGetValue
        if (RegisteredMainToAliasTypes.ContainsKey(mainType))
        {
            aliasTypes = RegisteredMainToAliasTypes[mainType];

            return true;
        }

        // If the requested main type is an interface and could not be found in the list of main types,
        // a second search for alias types of these interface should be started.
        // i.e. IWhatEverEntity is the registered main type, IWhatEver is the alias of it and the requested main type.
        // Now all non-interface alias of IWhatEverEntity should be retrieved.
        if (mainType.IsInterface
            && RegisteredAliasToMainTypes.TryGetValue(mainType, out List<Type> interfaceTypes)
            && interfaceTypes.Any(t => RegisteredMainToAliasTypes.ContainsKey(t)))
        {
            aliasTypes =
                interfaceTypes.Where(t => RegisteredMainToAliasTypes.ContainsKey(t))
                    .SelectMany(t => RegisteredMainToAliasTypes[t])
                    .Where(t => t != mainType)
                    .ToList();

            return true;
        }

        aliasTypes = default;

        return false;
    }

    public IList<string> GetQueryDocumentCollections()
    {
        return SavedInfos
            .Values
            .Select(c => c.QueryCollectionName)
            .Where(c => c != null)
            .Distinct(StringComparer.Ordinal)
            .ToList();
    }

    public IList<string> GetDocumentCollections()
    {
        return SavedInfos
            .Values
            .SelectMany(c => c.GetCollectionNames())
            .Where(c => c != null)
            .Distinct(StringComparer.Ordinal)
            .ToList();
    }

    public IList<string> GetEdgeCollections()
    {
        return SavedInfos
            .Values
            .SelectMany(
                c => c.Relations
                    .Select(r => r.EdgeCollection))
            .Distinct(StringComparer.Ordinal)
            .ToList();
    }

    public string GetCollectionName(Type type)
    {
        return GetValue(type)?.GetCollectionName(type);
    }

    public string GetCollectionName<TEntity>()
    {
        return GetCollectionName(typeof(TEntity));
    }

    public string GetQueryCollectionName(Type type)
    {
        return GetValue(type)?.QueryCollectionName
            ?? throw new Exception(
                $"No query collection name has been mapped to type '{type?.Name ?? "unknown"}' (regarding model builder).");
    }

    public string GetQueryCollectionName<TEntity>()
    {
        return GetQueryCollectionName(typeof(TEntity));
    }

    internal string GetKeyIdentifier(Type type)
    {
        return GetValue(type)?.KeyPropertyName;
    }

    public string GetKeyIdentifier<TEntity>()
    {
        return GetKeyIdentifier(typeof(TEntity));
    }

    internal (string property, object matchingValue) GetTypeInformation(Type entityType)
    {
        (string propertyName, object propertyValue)?
            typeInfo = GetValue(entityType)?.GetTypeInformation(entityType);

        if (typeInfo?.propertyName == null || typeInfo.Value.propertyName.StartsWith("/"))
        {
            return (null, null);
        }

        return (
            string.IsNullOrWhiteSpace(typeInfo.Value.propertyName) ? "types" : typeInfo.Value.propertyName,
            typeInfo.Value.propertyValue ?? entityType.Name
        );
    }

    internal string[] GetRelatedInboundEdgeCollections(Type type)
    {
        return GetValue(type)?.GetFromRelations(type, m => m.EdgeCollection).ToArray() ?? Array.Empty<string>();
    }

    internal string[] GetRelatedInboundEdgeCollections<TFrom>()
    {
        return GetRelatedInboundEdgeCollections(typeof(TFrom));
    }

    internal string[] GetRelatedOutboundEdgeCollections<TTo>()
    {
        return GetRelatedOutboundEdgeCollections(typeof(TTo));
    }

    internal string[] GetRelatedOutboundEdgeCollections(Type type)
    {
        return GetValue(type)?.GetToRelations(type, m => m.EdgeCollection).ToArray() ?? Array.Empty<string>();
    }

    public ModelBuilderOptionsTypeRelation GetRelation<TFrom, TTo>()
    {
        return GetRelation(typeof(TFrom), typeof(TTo));
    }

    public ModelBuilderOptionsTypeRelation GetRelation(Type fromType, Type toType)
    {
        ModelBuilderOptionsType value = GetValue(fromType);

        if (value == null)
        {
            return null;
        }

        fromType = value.MainType;
        ModelBuilderOptionsTypeRelation result = value.GetRelation(fromType, toType);

        if (result != null)
        {
            return result;
        }

        if (!TryGetMainTypes(toType, out IReadOnlyCollection<Type> aliasTypes))
        {
            return null;
        }

        return aliasTypes.Select(aliasType => value.GetRelation(fromType, aliasType))
            .FirstOrDefault(relation => relation != null);
    }

    internal IList<TypePropertyType> GetPropertyMapperInformation(
        Type entityType,
        Type targetTye)
    {
        if (GetValue(entityType)?.PropertyMapperConfiguration == null)
        {
            return null;
        }

        if (!GetValue(entityType)
                .PropertyMapperConfiguration
                .TryGetValue(targetTye, out List<TypePropertyType> mapping)
            || mapping == null)
        {
            return null;
        }

        return mapping;
    }
}
