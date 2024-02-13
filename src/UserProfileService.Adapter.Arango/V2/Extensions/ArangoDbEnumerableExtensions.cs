using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.Annotations;
using Maverick.UserProfileService.Models.BasicModels;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using Maverick.UserProfileService.Models.RequestModels;
using UserProfileService.Adapter.Arango.V2.Abstractions;
using UserProfileService.Adapter.Arango.V2.Annotations;
using UserProfileService.Adapter.Arango.V2.Contracts;
using UserProfileService.Adapter.Arango.V2.EntityModels;
using UserProfileService.Adapter.Arango.V2.EntityModels.QueryBuilding;
using UserProfileService.Adapter.Arango.V2.Helpers;
using UserProfileService.Common.V2.Extensions;

namespace UserProfileService.Adapter.Arango.V2.Extensions;

/// <summary>
///     Extension class that provides extension methods for querying <see cref="IArangoDbEnumerable{TEntity}"/>s.
/// </summary>
public static class ArangoDbEnumerableExtensions
{
    private static bool TryGenerateOrderExpression<TEntity>(
        string orderBy,
        SortOrder sortOrder,
        out Expression result)
    {
        string orderByProperty = GetCorrectPropertyName<TEntity>(orderBy);

        result = default;

        if (orderByProperty == default)
        {
            return false;
        }

        result = Expression.Lambda<Func<TEntity, Expression>>(
            new OrderByExpression(
                Expression.Constant(orderByProperty),
                sortOrder),
            new List<ParameterExpression>
            {
                Expression.Parameter(
                    typeof(TEntity),
                    "e")
            });

        return true;
    }

    private static bool TryGenerateOrderExpression(
        Type entityType,
        string orderBy,
        SortOrder sortOrder,
        out Expression result,
        Type interfaceType = null)
    {
        Type type = interfaceType != null && interfaceType.IsAssignableFrom(entityType)
            ? interfaceType
            : entityType;

        if (type == null)
        {
            throw new ArgumentNullException(nameof(entityType));
        }

        string orderByProperty = GetCorrectPropertyName(entityType, orderBy);

        result = default;

        if (orderByProperty == default)
        {
            return false;
        }

        result = Expression.Lambda(
            typeof(Func<,>).MakeGenericType(type, typeof(Expression)),
            new OrderByExpression(
                Expression.Constant(orderByProperty),
                sortOrder),
            new List<ParameterExpression>
            {
                Expression.Parameter(
                    type,
                    "e")
            });

        return true;
    }

    private static string GetCorrectPropertyName<TEntity>(string rawValue)
    {
        return GetCorrectPropertyName(typeof(TEntity), rawValue);
    }

    private static string GetCorrectPropertyName(
        Type entityType,
        string rawValue)
    {
        if (entityType == null)
        {
            throw new ArgumentNullException(nameof(entityType));
        }

        if (string.IsNullOrWhiteSpace(rawValue) || !rawValue.Contains("."))
        {
            return rawValue != null
                ? entityType
                    .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .FirstOrDefault(p => p.Name.Equals(rawValue, StringComparison.OrdinalIgnoreCase))
                    ?
                    .Name
                : default;
        }

        string[] propertyNames = rawValue.Split('.');
        var correctPropertyNames = new List<string>();
        Type objectType = entityType;

        foreach (string propertyName in propertyNames)
        {
            PropertyInfo correctProperty = objectType?.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .FirstOrDefault(p => p.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase));

            string correctPropertyName = GetCorrectPropertyName(
                objectType,
                correctProperty?.Name);

            if (correctPropertyName == null)
            {
                throw new ValidationException(
                    $"{objectType} doesn't have the property {propertyName} and cannot be sorted by it");
            }

            correctPropertyNames.Add(correctPropertyName);
            objectType = correctProperty?.PropertyType;
        }

        return correctPropertyNames.Any(string.IsNullOrEmpty) ? default : string.Join('.', correctPropertyNames);
    }

    private static IArangoDbEnumerable<TEntity> UsingOptionsInternal<TEntity, TQueryObject>(
        this IArangoDbEnumerable<TEntity> enumerable,
        TQueryObject filterObject,
        object argument = null,
        Func<ArangoDbEnumerable<TEntity>, string, TQueryObject, object, bool> filterProcessor = null)
        where TQueryObject : IQueryObject
    {
        string lastRequestId = enumerable.GetTypedEnumerable().LastRequestId;

        var next = new ArangoDbEnumerable<TEntity>(
            enumerable,
            new CombinedQuerySettings
            {
                CombinedByAnd = true
            });

        bool? filterAdded = filterProcessor?.Invoke(next, lastRequestId, filterObject, argument);

        next.AddPaginationSettings(filterObject);

        next.AddSortingSettings(filterObject);

        if ((!filterAdded.HasValue || !filterAdded.Value)
            && next is IArangoDbEnumerable<IProfileEntityModel> converted
            && argument is RequestedProfileKind reqKind)
        {
            converted.Where(reqKind);
        }

        return next;
    }

    private static bool AddObjectGenericFilterToWhereClause<TEntity>(
        ArangoDbEnumerable<TEntity> enumerable,
        string lastRequestId,
        QueryObject filterObject,
        object argument)
    {
        if (filterObject == null)
        {
            return false;
        }

        filterObject.Validate();

        Func<Type, bool> typeFilter = null;

        if (argument is not RequestedProfileKind reqKind)
        {
            reqKind = RequestedProfileKind.All;
        }
        else
        {
            typeFilter = GetTypeFilter(reqKind);
        }

        List<ExpressionDetails> filterToAdd =
            filterObject.Filter?.Definition?.Select(
                    def => GetFilterDefinitionItemForWhereClause(
                        enumerable,
                        def,
                        typeFilter))
                .Where(e => e != default)
                .ToList();

        if (filterToAdd != null && filterToAdd.Any())
        {
            enumerable.WhereExpressions.Add(
                new NestedExpressionDetails(
                    enumerable.LastRequestId,
                    filterToAdd)
                {
                    BinaryOperator = filterObject.Filter.CombinedBy
                });
        }

        bool stringFilterAdded =
            enumerable.AddStringFilterToWhereClause(lastRequestId, filterObject.Search, reqKind);

        bool tagFilterAdded = enumerable.AddMainTagFilter(
            lastRequestId,
            filterObject.TagFilters);

        return (filterToAdd != null && filterToAdd.Any()) || stringFilterAdded || tagFilterAdded;
    }

    private static Func<Type, bool> GetTypeFilter(RequestedProfileKind kind)
    {
        if (kind == RequestedProfileKind.Group)
        {
            return t => t == typeof(GroupEntityModel);
        }

        if (kind == RequestedProfileKind.User)
        {
            return t => t == typeof(UserEntityModel);
        }

        return null;
    }

    private static ExpressionDetails GetFilterDefinitionItemForWhereClause<TEntity>(
        ArangoDbEnumerable<TEntity> enumerable,
        Definitions definition,
        Func<Type, bool> typeFilter = null)
    {
        if (!definition.FieldName.Contains('.'))
        {
            return GetSingleFilterDefinitionItemForWhereClause(
                enumerable,
                definition,
                typeFilter: typeFilter);
        }

        string[] parts =
            definition.FieldName.Split('.', StringSplitOptions.RemoveEmptyEntries);

        PropertyInfo currentPropertyInfo = GetPropertyInfoByMainTypeAndAliasTypes(
            typeof(TEntity),
            parts[0].Trim(),
            enumerable.ModelSettings,
            $"(original property '{definition.FieldName}')");

        if (currentPropertyInfo == null)
        {
            throw new ValidationException(
                $"No property called '{parts[0]}' (original property '{definition.FieldName}') was found for type '{typeof(TEntity).Name}'.")
            {
                Data =
                {
                    { "RequestError", true },
                    { "InvalidFilter", true },
                    { "MissingField", definition.FieldName }
                }
            };
        }

        List<NestedPropertyInformation> nestedProperties = GetNestedPropertyInformation(
                typeof(TEntity),
                parts,
                $"(original type: {typeof(TEntity).Name}; complete field name: {definition.FieldName})",
                1,
                enumerable.ModelSettings)
            ?
            .ToList();

        return GetSingleFilterDefinitionItemForWhereClause(
            enumerable,
            new Definitions
            {
                BinaryOperator = definition.BinaryOperator,
                FieldName = parts[0],
                Values = definition.Values,
                Operator = definition.Operator
            },
            nestedProperties?
                .FirstOrDefault(p => p.MethodToUse != null)
                ?
                .PropertyName,
            e => e.NestedPropertyInformation = nestedProperties,
            typeFilter,
            nestedProperties?.Count > 0);
    }

    private static ExpressionDetails GetSingleFilterDefinitionItemForWhereClause<TEntity>(
        ArangoDbEnumerable<TEntity> enumerable,
        Definitions definition,
        string convertToMethodName = null,
        Action<ExpressionDetails> changeExpressionDetails = null,
        Func<Type, bool> typeFilter = null,
        bool executeDeepQueryMethod = false)

    {
        if (definition == null)
        {
            throw new ArgumentNullException(nameof(definition));
        }

        if (string.IsNullOrWhiteSpace(definition.FieldName))
        {
            throw new ArgumentException(
                "Malformed definition item. The containing field name is null, empty or whitespace, but should not.",
                nameof(definition));
        }

        PropertyInfo relevantProperty = GetProperty(typeof(TEntity), definition.FieldName);

        if (relevantProperty != default)
        {
            return GetLambdaExpressionDetails(
                enumerable.LastRequestId,
                definition,
                typeof(TEntity),
                relevantProperty,
                convertToMethodName,
                changeExpressionDetails,
                executeDeepQueryMethod);
        }

        if (!enumerable.ModelSettings.TryGetAliasTypes(typeof(TEntity), out IReadOnlyCollection<Type> aliasTypes)
            || !TryGetRelatedTypes(typeof(TEntity), aliasTypes, out List<Type> relatedTypes)
            || !TryGet(enumerable.TypeFilter, relatedTypes, out List<Type> filteredTypes))
        {
            throw new ValidationException(
                $"No property called '{definition.FieldName}' was found for type '{typeof(TEntity).Name}'.")
            {
                Data =
                {
                    { "RequestError", true },
                    { "InvalidFilter", true },
                    { "MissingField", definition.FieldName }
                }
            };
        }

        List<LambdaExpression> generatedExpressions =
            filteredTypes
                .WhereSafely(typeFilter)
                .Select(
                    relatedType => GetLambdaExpressionSafely(
                        definition,
                        relatedType,
                        GetProperty(relatedType, definition.FieldName),
                        convertToMethodName,
                        executeDeepQueryMethod))
                .Where(expr => expr != default)
                .ToList();

        var combined = new ExpressionDetails(enumerable.LastRequestId);
        changeExpressionDetails?.Invoke(combined);
        combined.BatchedExpressions.AddRange(generatedExpressions);

        if (generatedExpressions.All(le => le == null))
        {
            throw new ValidationException(
                $"No property called '{definition.FieldName}' was found for type '{typeof(TEntity).Name}', "
                + $"nor for related types '{string.Join("','", filteredTypes.WhereSafely(typeFilter).Select(t => t.Name))}'.")
            {
                Data =
                {
                    { "RequestError", true },
                    { "InvalidFilter", true },
                    { "MissingField", definition.FieldName }
                }
            };
        }

        return combined;

        static bool TryGet(
            List<string> typeFilter,
            List<Type> inputTypes,
            out List<Type> types)
        {
            if (typeFilter == null || typeFilter.Count == 0)
            {
                types = inputTypes;

                return true;
            }

            if (inputTypes == null)
            {
                types = null;

                return false;
            }

            Type found = inputTypes.FirstOrDefault(
                t =>
                    t?.Name != null && typeFilter.Contains(t.Name, StringComparer.OrdinalIgnoreCase));

            types = found != null
                ? new List<Type>
                {
                    found
                }
                : null;

            return found != null;
        }
    }

    private static bool TryGetRelatedTypes(
        Type entityType,
        IEnumerable<Type> readOnlyCollection,
        out List<Type> related)
    {
        related = readOnlyCollection?.Where(entityType.IsAssignableFrom).ToList();

        return related is
        {
            Count: > 0
        };
    }

    private static PropertyInfo GetProperty(
        Type entityType,
        string fieldName)
    {
        return entityType?
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .FirstOrDefault(p => p.Name.Equals(fieldName, StringComparison.OrdinalIgnoreCase));
    }

    private static bool TryGetPropertyInfoByMainTypeAndAliasTypes(
        Type entityType,
        string propertyName,
        ModelBuilderOptions modelSettings,
        Func<Type, bool> typeFilter,
        out PropertyInfo property,
        out List<Type> relatedTypes)
    {
        property = null;
        relatedTypes = null;

        if (string.IsNullOrWhiteSpace(propertyName) || modelSettings == null || entityType == null)
        {
            return false;
        }

        property = GetProperty(entityType, propertyName);

        if (property != null)
        {
            return true;
        }

        if (!modelSettings.TryGetAliasTypes(entityType, out IReadOnlyCollection<Type> aliasTypes)
            || !TryGetRelatedTypes(entityType, aliasTypes, out relatedTypes))
        {
            return false;
        }

        foreach (Type relatedType in relatedTypes)
        {
            if (typeFilter != null && !typeFilter.Invoke(relatedType))
            {
                continue;
            }

            property = GetProperty(relatedType, propertyName);

            if (property != null)
            {
                return true;
            }
        }

        return false;
    }

    private static IEnumerable<NestedPropertyInformation> GetNestedPropertyInformation(
        Type startingType,
        IEnumerable<string> propertyNames,
        string errorInfoOfOriginalRequest,
        int skipAmount,
        ModelBuilderOptions modelSettings)
    {
        Type currentType = startingType;
        Type parentType = null; // either type that "owns" the property or a list/array, that wraps the type
        var currentLoop = 0;

        foreach (string part in propertyNames)
        {
            if (TryGetWellKnownNestedPropertyInfo(parentType, part, out NestedPropertyInformation propertyInfo))
            {
                yield return propertyInfo;

                yield break;
            }

            PropertyInfo currentPropertyInfo = GetPropertyInfoByMainTypeAndAliasTypes(
                currentType,
                part,
                modelSettings,
                errorInfoOfOriginalRequest);

            bool isList = typeof(IEnumerable).IsAssignableFrom(currentPropertyInfo.PropertyType)
                && currentPropertyInfo.PropertyType != typeof(string);

            parentType = isList
                ? currentPropertyInfo.PropertyType
                : currentType;

            currentType = isList
                ? currentPropertyInfo.PropertyType.GenericTypeArguments.FirstOrDefault()
                : currentPropertyInfo.PropertyType;

            if (currentLoop++ < skipAmount)
            {
                continue;
            }

            yield return new NestedPropertyInformation
            {
                IsList = isList,
                PropertyName = currentPropertyInfo.Name
            };
        }
    }

    private static bool TryGetWellKnownNestedPropertyInfo(
        Type parentType,
        string propertyName,
        out NestedPropertyInformation result)
    {
        if (!HasMappedMethodInsteadProperty(parentType, propertyName))
        {
            result = default;

            return false;
        }

        result = new NestedPropertyInformation
        {
            IsList = false,
            PropertyName = propertyName,
            MethodToUse = WellKnownFilterProperties.EnumerablePropertyMapping[propertyName]
        };

        return true;
    }

    // in this context the field name in the filter object will be treated as a member name in C#/AQL
    private static bool HasMappedMethodInsteadProperty(Type relatedType, string fieldName)
    {
        return relatedType != null
            && !string.IsNullOrEmpty(fieldName)
            && typeof(IEnumerable).IsAssignableFrom(relatedType)
            && WellKnownFilterProperties.EnumerablePropertyMapping.ContainsKey(fieldName);
    }

    private static PropertyInfo GetPropertyInfoByMainTypeAndAliasTypes(
        Type type,
        string propertyName,
        ModelBuilderOptions modelSettings,
        string additionalErrorText)
    {
        if (!TryGetPropertyInfoByMainTypeAndAliasTypes(
                type,
                propertyName,
                modelSettings,
                null,
                out PropertyInfo propertyInfo,
                out List<Type> relatedTypes))
        {
            throw new ValidationException(
                string.Concat(
                    $"No property called '{propertyName}' was found for type '{type?.Name}' {additionalErrorText}. ",
                    "Nor for related types",
                    relatedTypes != null
                        ? string.Join(",", relatedTypes.Select(t => t.Name))
                        : string.Empty,
                    "."))
            {
                Data =
                {
                    { "RequestError", true },
                    { "InvalidFilter", true },
                    { "MissingField", propertyName }
                }
            };
        }

        return propertyInfo;
    }

    private static LambdaExpression GetLambdaExpressionSafely(
        Definitions definition,
        Type entityTypeNew,
        PropertyInfo propertyInfo,
        string convertToMethodName = null,
        bool executeDeepQueryMethod = false)
    {
        if (propertyInfo == null)
        {
            // execution is invalid,  if property info is null. Else it is ok
            return default;
        }

        if (TryGetVirtualPropertyInformation(
                propertyInfo,
                out PropertyInfo realPropertyInfo,
                out string filterPropertyName,
                out object filterPropertyValue,
                out IVirtualPropertyResolver resolver))
        {
            propertyInfo = realPropertyInfo;
        }

        LambdaExpression lambdaExpression = GetSpecificLambdaExpression(
            entityTypeNew,
            propertyInfo,
            Expression.Parameter(entityTypeNew, convertToMethodName ?? "propertyFilter"),
            Expression.Constant(definition.Values),
            definition.Operator,
            definition.BinaryOperator == BinaryOperator.And,
            filterPropertyName,
            filterPropertyValue,
            resolver,
            executeDeepQueryMethod);

        return lambdaExpression;
    }

    private static bool TryGetVirtualPropertyInformation(
        PropertyInfo propertyInfo,
        out PropertyInfo realPropertyInfo,
        out string filterPropertyName,
        out object filterPropertyValue,
        out IVirtualPropertyResolver virtualPropertyResolver)
    {
        realPropertyInfo = default;
        filterPropertyName = default;
        filterPropertyValue = default;
        virtualPropertyResolver = default;

        if (propertyInfo?
                    .GetCustomAttributes(typeof(VirtualPropertyAttribute), true)
                    .FirstOrDefault(attr => attr is VirtualPropertyAttribute)
                is not VirtualPropertyAttribute virtualPropertyAttribute
            || virtualPropertyAttribute.ParentType == null
            || string.IsNullOrWhiteSpace(virtualPropertyAttribute.NameRealProperty))
        {
            return false;
        }

        realPropertyInfo =
            virtualPropertyAttribute.ParentType.GetProperty(
                virtualPropertyAttribute.NameRealProperty,
                BindingFlags.Instance | BindingFlags.Public)
            ?? throw new Exception(
                $"Internal error: Missing real property '{virtualPropertyAttribute.ParentType.Name}' in class '{virtualPropertyAttribute.NameRealProperty}'!");

        virtualPropertyResolver = virtualPropertyAttribute.Resolver;

        if (string.IsNullOrWhiteSpace(virtualPropertyAttribute.FilterPropertyName)
            || virtualPropertyAttribute.FilterPropertyValue == null)
        {
            return true;
        }

        filterPropertyName = virtualPropertyAttribute.FilterPropertyName;
        filterPropertyValue = virtualPropertyAttribute.FilterPropertyValue;

        return true;
    }

    private static ExpressionDetails GetLambdaExpressionDetails(
        string lastRequestId,
        Definitions definition,
        Type entityTypeNew,
        PropertyInfo propertyInfo,
        string convertToMethodName = null,
        Action<ExpressionDetails> changeExpressionDetails = null,
        bool executeDeepQueryMethod = false
    )
    {
        if (propertyInfo == null)
        {
            // execution is invalid,  if property info is null. Else it is ok
            return default;
        }

        LambdaExpression lambdaExpression =
            GetSpecificLambdaExpression(
                entityTypeNew,
                propertyInfo,
                Expression.Parameter(entityTypeNew, convertToMethodName ?? "propertyFilter"),
                Expression.Constant(definition.Values),
                definition.Operator,
                definition.BinaryOperator == BinaryOperator.And,
                null,
                null,
                null,
                executeDeepQueryMethod);

        if (lambdaExpression == null)
        {
            return default;
        }

        var resultingExpression = new ExpressionDetails(
            lastRequestId,
            lambdaExpression);

        changeExpressionDetails?.Invoke(resultingExpression);

        return resultingExpression;
    }

    private static LambdaExpression GetSpecificLambdaExpression(
        Type entityType,
        PropertyInfo relevantProperty,
        ParameterExpression parameterExpression,
        Expression rightSide,
        FilterOperator @operator,
        bool allMustBeContained,
        string filterPropertyName,
        object filterPropertyValue,
        IVirtualPropertyResolver resolver,
        bool executeDeepQueryMethod = false)
    {
        if (!HasMappedMethodInsteadProperty(relevantProperty.PropertyType, parameterExpression.Name)
            && typeof(IEnumerable).IsAssignableFrom(relevantProperty.PropertyType)
            && relevantProperty.PropertyType != typeof(string)
            && @operator != FilterOperator.Contains
            && (resolver?.GetReturnType() == null || !resolver.GetReturnType().IsPrimitive)
           )
        {
            MethodInfo methodInfo = typeof(ComparingHelpers)
                    .GetMethod(
                        nameof(ComparingHelpers.SequenceEqual),
                        BindingFlags.Static | BindingFlags.NonPublic,
                        null,
                        CallingConventions.Standard,
                        new[] { typeof(IEnumerable), typeof(IEnumerable), typeof(string), typeof(object) },
                        null)
                ?? throw new Exception(
                    $"Internal error in GetSpecificLambdaExpression(type: {relevantProperty.PropertyType.Name})! Wrong method signature used!");

            return Expression.Lambda(
                typeof(Func<,>).MakeGenericType(entityType, typeof(bool)),
                Expression.Call(
                    null,
                    methodInfo,
                    Expression.Property(parameterExpression, relevantProperty),
                    rightSide,
                    !string.IsNullOrWhiteSpace(filterPropertyName)
                        ? Expression.Constant(filterPropertyName)
                        : Expression.Constant(null, typeof(string)),
                    Expression.Constant(filterPropertyValue)),
                parameterExpression);
        }

        if (typeof(IEnumerable).IsAssignableFrom(relevantProperty.PropertyType)
            && relevantProperty.PropertyType != typeof(string)
            && @operator == FilterOperator.Contains)
        {
            MethodInfo methodInfo = typeof(ComparingHelpers)
                    .GetMethod(
                        nameof(ComparingHelpers.CheckExistenceOfElementsInTwoSequences),
                        BindingFlags.Static | BindingFlags.NonPublic,
                        null,
                        CallingConventions.Standard,
                        new[]
                        {
                            typeof(IEnumerable), typeof(IEnumerable), typeof(bool), typeof(string), typeof(object)
                        },
                        null)
                ?? throw new Exception(
                    $"Internal error in GetSpecificLambdaExpression(type: {relevantProperty.PropertyType.Name})! Wrong method signature used!");

            return Expression.Lambda(
                typeof(Func<,>).MakeGenericType(entityType, typeof(bool)),
                Expression.Call(
                    null,
                    methodInfo,
                    rightSide, // in this case it is the static variable that should be checked, not the property (i.e. values)
                    Expression.Property(parameterExpression, relevantProperty),
                    Expression.Constant(allMustBeContained),
                    !string.IsNullOrWhiteSpace(filterPropertyName)
                        ? Expression.Constant(filterPropertyName)
                        : Expression.Constant(null, typeof(string)),
                    Expression.Constant(filterPropertyValue)),
                parameterExpression);
        }

        if (relevantProperty.PropertyType.IsPrimitive
            || relevantProperty.PropertyType == typeof(string)
            || relevantProperty.PropertyType == typeof(DateTime)
            || relevantProperty.PropertyType == typeof(DateTime?)
            || HasMappedMethodInsteadProperty(relevantProperty.PropertyType, parameterExpression.Name)
            || (resolver?.GetReturnType() != null && resolver.GetReturnType().IsPrimitive)
            || executeDeepQueryMethod)
        {
            Type[] inputTypes = resolver != null
                ? new[]
                {
                    typeof(object),
                    typeof(IEnumerable<string>),
                    typeof(FilterOperator),
                    typeof(bool),
                    typeof(IVirtualPropertyResolver)
                }
                : new[]
                {
                    typeof(object),
                    typeof(IEnumerable<string>),
                    typeof(FilterOperator),
                    typeof(bool),
                    typeof(string),
                    typeof(object)
                };

            MethodInfo methodInfo = typeof(ComparingHelpers)
                    .GetMethod(
                        nameof(ComparingHelpers.ContainsValueOf),
                        BindingFlags.NonPublic | BindingFlags.Static,
                        null,
                        CallingConventions.Standard,
                        inputTypes,
                        null)
                ?? throw new Exception(
                    $"Internal error in GetSpecificLambdaExpression(type: {relevantProperty.PropertyType.Name})! Wrong method signature used!");

            UnaryExpression conversionExpression = Expression.Convert(
                Expression.Property(
                    parameterExpression,
                    relevantProperty),
                typeof(object));

            Expression[] inputExpressions = resolver != null
                ? new[]
                {
                    conversionExpression,
                    rightSide,
                    Expression.Constant(@operator),
                    Expression.Constant(allMustBeContained),
                    Expression.Constant(resolver)
                }
                : new[]
                {
                    conversionExpression,
                    rightSide,
                    Expression.Constant(@operator),
                    Expression.Constant(allMustBeContained),
                    !string.IsNullOrWhiteSpace(filterPropertyName)
                        ? Expression.Constant(filterPropertyName)
                        : Expression.Constant(null, typeof(string)),
                    Expression.Constant(filterPropertyValue)
                };

            return Expression.Lambda(
                typeof(Func<,>).MakeGenericType(entityType, typeof(bool)),
                Expression.Call(
                    null,
                    methodInfo,
                    inputExpressions),
                parameterExpression);
        }

        return null;
    }

    private static bool AddObjectListToWhereClause<TEntity>(
        ArangoDbEnumerable<TEntity> enumerable,
        string lastRequestId,
        QueryObjectList filterObject,
        object argument)
    {
        if (filterObject == null)
        {
            return false;
        }

        bool tagFilterAdded = enumerable.AddTagFilterToWhereClause(lastRequestId, filterObject);

        bool kindFilterAlreadySet = enumerable.AddStringFilterToWhereClause(
            lastRequestId,
            filterObject.Filter,
            filterObject.ProfileKind);

        if (!kindFilterAlreadySet
            && filterObject.ProfileKind != RequestedProfileKind.All
            && filterObject.ProfileKind != RequestedProfileKind.Undefined
            && typeof(IProfileEntityModel).IsAssignableFrom(typeof(TEntity)))
        {
            LambdaExpression kindFilter = Expression.Lambda(
                RawQueryExpression.CreateInstance<IProfileEntityModel, ProfileKind>(
                    o => $"{o}==\"{filterObject.ProfileKind.Convert():G}\"",
                    e => e.Kind),
                Expression.Parameter(typeof(TEntity), "p"));

            enumerable.WhereExpressions
                .Add(new ExpressionDetails(lastRequestId, kindFilter, true));

            return true;
        }

        return tagFilterAdded || kindFilterAlreadySet;
    }

    private static void AddPaginationSettings<TEntity, TQueryObject>(
        this ArangoDbEnumerable<TEntity> existing,
        TQueryObject filterObject)
        where TQueryObject : IQueryObject
    {
        if (filterObject?.Limit > 0 || filterObject?.Offset >= 0)
        {
            existing.GetEnumerable().Limit = filterObject.Limit;
            existing.GetEnumerable().Offset = filterObject.Offset;
        }
    }

    private static void AddSortingSettings<TEntity, TQueryObject>(
        this ArangoDbEnumerable<TEntity> existing,
        TQueryObject filterObject)
        where TQueryObject : IQueryObject
    {
        if (!string.IsNullOrEmpty(filterObject?.OrderedBy)
            && TryGenerateOrderExpressionIncludingAliasTypes(
                existing,
                filterObject.OrderedBy,
                filterObject.SortOrder,
                out Expression lambdaExpr))
        {
            existing.OrderExpressions.Add(lambdaExpr);
        }
    }

    private static bool TryGenerateOrderExpressionIncludingAliasTypes<TEntity>(
        ArangoDbEnumerable<TEntity> existing,
        string orderBy,
        SortOrder sortOrder,
        out Expression expression)
    {
        expression = default;

        if (string.IsNullOrWhiteSpace(orderBy))
        {
            return false;
        }

        if (TryGenerateOrderExpression<TEntity>(orderBy, sortOrder, out Expression firstLambda))
        {
            expression = firstLambda;

            return true;
        }

        if (existing?.ModelSettings == null
            || !existing.ModelSettings.TryGetAliasTypes(typeof(TEntity), out IReadOnlyCollection<Type> aliasTypes))
        {
            return false;
        }

        foreach (Type aliasType in aliasTypes)
        {
            if (TryGenerateOrderExpression(
                    aliasType,
                    orderBy,
                    sortOrder,
                    out Expression lambdaExpression,
                    typeof(TEntity)))
            {
                expression = lambdaExpression;

                return true;
            }
        }

        return false;
    }

    /// <summary>
    ///     Adds a tags filter to an existing where clause expression, if necessary.
    /// </summary>
    private static bool AddTagFilterToWhereClause<TEntity>(
        this ArangoDbEnumerable<TEntity> existing,
        string lastRequestId,
        QueryObjectList filterObject)
    {
        return existing.AddTagFilterToWhereClause(lastRequestId, filterObject?.Tags);
    }

    /// <summary>
    ///     Adds a tags filter to an existing where clause expression, if necessary.
    /// </summary>
    private static bool AddTagFilterToWhereClause<TEntity>(
        this ArangoDbEnumerable<TEntity> existing,
        string lastRequestId,
        IList<string> tagsToBeFiltered)
    {
        if (tagsToBeFiltered != null
            && typeof(ITagsIncludedObject).IsAssignableFrom(typeof(TEntity))
            && tagsToBeFiltered.Any(t => !string.IsNullOrWhiteSpace(t)))
        {
            Expression<Func<TEntity, bool>> tagFilter = entity =>
                tagsToBeFiltered.AllTagsIncludedIn(((ITagsIncludedObject)entity).Tags);

            existing.WhereExpressions.Add(new ExpressionDetails(lastRequestId, tagFilter));

            return true;
        }

        return false;
    }

    /// <summary>
    ///     Adds a string filter to existing <see cref="ArangoDbEnumerable" /> object, if necessary.<br />
    ///     It will return a boolean value indicating whether a kind filter has been already set or not.
    /// </summary>
    private static bool AddStringFilterToWhereClause<TEntity>(
        this ArangoDbEnumerable<TEntity> existing,
        string lastRequestId,
        string filterString,
        RequestedProfileKind profileKind =
            RequestedProfileKind.All)
    {
        if (string.IsNullOrWhiteSpace(filterString))
        {
            return false;
        }

        List<LambdaExpression> expressions = GetLambdaFilterExpressions<TEntity>(
                filterString,
                profileKind)
            .Where(e => e != null)
            .ToList();

        if (expressions.Count > 0)
        {
            existing.WhereExpressions.Add(new ExpressionDetails(lastRequestId, expressions));

            return true;
        }

        Expression finalLambdaExpression;

        if (typeof(TEntity) == typeof(IContainerProfileEntityModel)
            || typeof(IContainerProfileEntityModel).IsAssignableFrom(typeof(TEntity)))
        {
            finalLambdaExpression = GetSearchableExpression<IContainerProfileEntityModel>(filterString);
        }
        else if (typeof(TEntity) == typeof(IProfileEntityModel)
                 || typeof(IProfileEntityModel).IsAssignableFrom(typeof(TEntity)))
        {
            finalLambdaExpression = GetSearchableExpression<IProfileEntityModel>(filterString);
        }
        else
        {
            finalLambdaExpression = GetSearchableExpression<TEntity>(filterString);
        }

        if (finalLambdaExpression != null)
        {
            existing.WhereExpressions.Add(
                new ExpressionDetails(lastRequestId, finalLambdaExpression)
                {
                    CombinedByAnd = false
                });

            return true;
        }

        return false;
    }

    private static bool AddMainTagFilter<TEntity>(
        this ArangoDbEnumerable<TEntity> existing,
        string lastRequestId,
        IEnumerable<string> tagList)
    {
        if (existing == null)
        {
            return false;
        }

        List<string> validTags = tagList?
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .ToList();

        if (validTags == null || validTags.Count == 0)
        {
            return false;
        }

        return existing.AddTagFilterToWhereClause(lastRequestId, validTags);
    }

    private static IEnumerable<LambdaExpression> GetLambdaFilterExpressions<TEntity>(
        string filterString,
        RequestedProfileKind profileKind)
    {
        if (typeof(TEntity) == typeof(User)
            || typeof(User).IsAssignableFrom(typeof(TEntity))
            || (typeof(TEntity) == typeof(IProfileEntityModel) && profileKind.HasFlag(RequestedProfileKind.User))
            || (typeof(IProfileEntityModel).IsAssignableFrom(typeof(TEntity))
                && profileKind.HasFlag(RequestedProfileKind.User)
                && typeof(TEntity) != typeof(IContainerProfileEntityModel)
                && typeof(TEntity) != typeof(IContainerProfile))
           )
        {
            yield return GetSearchableExpression<User>(filterString);
        }

        if (typeof(TEntity) == typeof(Group)
            || typeof(Group).IsAssignableFrom(typeof(TEntity))
            || typeof(TEntity) == typeof(IContainerProfileEntityModel)
            || (typeof(TEntity) == typeof(IProfileEntityModel) && profileKind.HasFlag(RequestedProfileKind.Group))
            || (typeof(IProfileEntityModel).IsAssignableFrom(typeof(TEntity))
                && profileKind.HasFlag(RequestedProfileKind.Group)))
        {
            yield return GetSearchableExpression<Group>(filterString);
        }

        if (typeof(TEntity) == typeof(Organization)
            || typeof(Organization).IsAssignableFrom(typeof(TEntity))
            || typeof(TEntity) == typeof(IContainerProfileEntityModel)
            || (typeof(TEntity) == typeof(IProfileEntityModel)
                && profileKind.HasFlag(RequestedProfileKind.Organization))
            || (typeof(IProfileEntityModel).IsAssignableFrom(typeof(TEntity))
                && profileKind.HasFlag(RequestedProfileKind.Organization)))
        {
            yield return GetSearchableExpression<Organization>(filterString);
        }

        if (typeof(TEntity) == typeof(FunctionBasic) || typeof(FunctionBasic).IsAssignableFrom(typeof(TEntity)))
        {
            yield return GetSearchableExpression<FunctionView>(filterString);
        }

        if (typeof(TEntity) == typeof(RoleBasic) || typeof(RoleBasic).IsAssignableFrom(typeof(TEntity)))
        {
            yield return GetSearchableExpression<RoleView>(filterString);
        }

        // In this context a fallback is not necessary 
    }

    private static LambdaExpression GetSearchableExpression<TEntity>(string filterString)
    {
        List<PropertyInfo> searchableProperties = GetSearchableProperties(typeof(TEntity));

        MethodInfo methodInfo = typeof(string)
                .GetMethod(
                    nameof(string.Contains),
                    BindingFlags.Public | BindingFlags.Instance,
                    null,
                    CallingConventions.HasThis,
                    new[] { typeof(string) },
                    null)
            ?? throw new MissingMethodException("Could not find method String.Contains()");

        ParameterExpression parameter = Expression.Parameter(typeof(TEntity), "propertySearch");

        if (searchableProperties.Count == 0)
        {
            return default;
        }

        Expression combinedExpression;

        // if only one property was found, a combined expression query is not necessary.
        if (searchableProperties.Count == 1)
        {
            combinedExpression = Expression.Call(
                Expression.Property(parameter, searchableProperties[0]),
                methodInfo,
                Expression.Constant(filterString));
        }
        else
        {
            combinedExpression = Expression.Call(
                Expression.Property(parameter, searchableProperties[0]),
                methodInfo,
                Expression.Constant(filterString));

            for (var i = 1; i < searchableProperties.Count; i++)
            {
                combinedExpression = Expression.MakeBinary(
                    ExpressionType.OrElse,
                    combinedExpression,
                    Expression.Call(
                        Expression.Property(parameter, searchableProperties[i]),
                        methodInfo,
                        Expression.Constant(filterString)));
            }
        }

        return Expression.Lambda(combinedExpression, parameter);

        static List<PropertyInfo> GetSearchableProperties(Type entityType)
        {
            List<PropertyInfo> propertyInfos =
                entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(
                        p => p.GetCustomAttributes(true)
                            .Any(att => att.GetType() == typeof(SearchableAttribute)))
                    .Where(pi => pi.PropertyType == typeof(string))
                    .ToList();

            return propertyInfos;
        }
    }

    internal static IArangoDbEnumerable<TEntity> Entity<TEntity>(this ModelBuilderOptions options)
    {
        if (!options.ContainsTypeDefinition<TEntity>())
        {
            throw new Exception(
                $"Unknown type for an entity '{typeof(TEntity).Name}'. Maybe it has not been registered.");
        }

        return new ArangoDbEnumerable<TEntity>(options);
    }

    internal static IArangoDbEnumerable<TEntity> Entity<TEntity>(this DefaultModelConstellation constellation)
    {
        if (constellation == null)
        {
            throw new ArgumentNullException(nameof(constellation));
        }

        return Entity<TEntity>(constellation.ModelsInfo);
    }

    /// <summary>
    ///     Retrieves the first element from an ArangoDB enumerable that satisfies the specified filter condition.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="enumerable">The ArangoDB enumerable.</param>
    /// <param name="filter">The filter expression.</param>
    /// <returns>An ArangoDB single enumerable containing the first matching element.</returns>
    public static IArangoDbSingleEnumerable<TEntity> First<TEntity>(
        this IArangoDbEnumerable<TEntity> enumerable,
        Expression<Func<TEntity, bool>> filter)
    {
        var next = new ArangoDbSingleEnumerable<TEntity>(enumerable)
        {
            Limit = 1,
            Offset = 0
        };

        next.WhereExpressions.Add(new ExpressionDetails(next.LastRequestId, filter));

        return next;
    }

    internal static IArangoDbEnumerable<TEntity> Select<TEntity, TResult>(
        this IArangoDbEnumerable<TEntity> enumerable,
        Expression<Func<TEntity, TResult>> selector)
    {
        var next = new ArangoDbEnumerable<TEntity>(enumerable);
        next.SelectExpressions.Add(selector);

        return next;
    }

    internal static IArangoDbEnumerable<TEntity> SortBy<TEntity>(
        this IArangoDbEnumerable<TEntity> enumerable,
        string propertyName,
        SortOrder sortOrder = SortOrder.Asc)
    {
        var next = new ArangoDbEnumerable<TEntity>(enumerable);

        if (!TryGenerateOrderExpressionIncludingAliasTypes(
                next,
                propertyName,
                sortOrder,
                out Expression lambdaExpression))
        {
            return enumerable;
        }

        next.OrderExpressions.Add(lambdaExpression);

        return next;
    }

    internal static IArangoDbEnumerable<TEntity> Skip<TEntity>(
        this IArangoDbEnumerable<TEntity> enumerable,
        int amount)
    {
        var next = new ArangoDbEnumerable<TEntity>(enumerable)
        {
            Offset = amount >= 0
                ? amount
                : 0
        };

        return next;
    }

    internal static IArangoDbEnumerable<TEntity> Take<TEntity>(
        this IArangoDbEnumerable<TEntity> enumerable,
        int amount)
    {
        var next = new ArangoDbEnumerable<TEntity>(enumerable)
        {
            Limit = amount > 0
                ? amount
                : 100
        };

        return next;
    }

    internal static IArangoDbEnumerable<TOutput> AsSubQueryIn<TInput, TOutput>(
        this IArangoDbEnumerable<TInput> enumerable,
        Func<IArangoDbEnumerable<TOutput>, IArangoDbEnumerable> outerQuery)
    {
        return new ArangoNestedQueryEnumerable<TOutput>(outerQuery, enumerable);
    }

    /// <summary>
    ///     Projects each element of an ArangoDB single enumerable into a new form using the specified selector.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TResult">The type of the result after projection.</typeparam>
    /// <param name="enumerable">The ArangoDB single enumerable.</param>
    /// <param name="selector">The selector expression.</param>
    /// <returns>An ArangoDB enumerable with the applied projection.</returns>
    public static IArangoDbEnumerable<TEntity> Select<TEntity, TResult>(
        this IArangoDbSingleEnumerable<TEntity> enumerable,
        Expression<Func<TEntity, TResult>> selector)
    {
        var next = new ArangoDbEnumerable<TEntity>(enumerable.GetTypedEnumerable());
        next.SelectExpressions.Add(selector);

        return next;
    }

    internal static IArangoDbEnumerable<TEntity> Where<TEntity>(
        this IArangoDbEnumerable<TEntity> enumerable,
        RequestedAssignmentObjectType type)
        where TEntity : IAssignmentObject
    {
        if (type is RequestedAssignmentObjectType.All or RequestedAssignmentObjectType.Undefined)
        {
            return enumerable;
        }

        var next = new ArangoDbEnumerable<TEntity>(
            enumerable,
            new CombinedQuerySettings
            {
                CombinedByAnd = true
            })
        {
            TypeFilter = type.GetEntityTypeNames()
        };

        RawQueryExpression rawExpressions = type.CreateRaqQueryExpression();

        LambdaExpression kindFilter =
            Expression.Lambda(rawExpressions, Expression.Parameter(typeof(TEntity), "p"));

        next.WhereExpressions.Add(new ExpressionDetails(next.LastRequestId, kindFilter));

        return next;
    }

    internal static IArangoDbEnumerable<TEntity> Where<TEntity>(
        this IArangoDbEnumerable<TEntity> enumerable,
        RequestedProfileKind profileKind)
        where TEntity : IProfileEntityModel
    {
        if (profileKind is RequestedProfileKind.All or RequestedProfileKind.Undefined)
        {
            return enumerable;
        }

        var next = new ArangoDbEnumerable<TEntity>(
            enumerable,
            new CombinedQuerySettings
            {
                CombinedByAnd = true
            })
        {
            TypeFilter = profileKind.GetEntityTypeNames()
        };

        RawQueryExpression rawExpressions = profileKind.CreateRaqQueryExpression();

        LambdaExpression kindFilter =
            Expression.Lambda(rawExpressions, Expression.Parameter(typeof(TEntity), "p"));

        next.WhereExpressions.Add(new ExpressionDetails(next.LastRequestId, kindFilter));

        return next;
    }

    // specific logic for members and parents
    internal static IArangoDbEnumerable<TEntity> WhereMemberOfIsEmptyValidFor<TEntity>(
        this IArangoDbEnumerable<TEntity> enumerable,
        RequestedProfileKind parentKind)
        where TEntity : IProfileEntityModel
    {
        Expression<Func<TEntity, bool>> lambda;

        switch (parentKind)
        {
            case RequestedProfileKind.Organization:
                lambda = profile => profile.MemberOf.Count(m => m.Kind == ProfileKind.Organization) == 0;

                break;
            case RequestedProfileKind.Group:
                lambda = profile => profile.MemberOf
                        .Count(m => m.Kind == ProfileKind.Group || m.Kind == ProfileKind.User)
                    == 0;

                break;
            default:
                return enumerable;
        }

        var next = new ArangoDbEnumerable<TEntity>(enumerable);
        next.WhereExpressions.Add(new ExpressionDetails(next.LastRequestId, lambda));

        return next;
    }

    /// <summary>
    ///     Filters the elements in the ArangoDB enumerable based on the specified condition.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="enumerable">The ArangoDB enumerable.</param>
    /// <param name="filter">The filter expression.</param>
    /// <returns>An ArangoDB enumerable with the added filter expression.</returns>
    public static IArangoDbEnumerable<TEntity> Where<TEntity>(
        this IArangoDbEnumerable<TEntity> enumerable,
        Expression<Func<TEntity, bool>> filter)
    {
        var next = new ArangoDbEnumerable<TEntity>(enumerable);
        next.WhereExpressions.Add(new ExpressionDetails(next.LastRequestId, filter));

        return next;
    }

    /// <summary>
    ///     Applies query options to an ArangoDB enumerable based on the specified <paramref name="options"/>.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="enumerable">The ArangoDB enumerable.</param>
    /// <param name="options">The query options to apply.</param>
    /// <param name="argument">Additional argument (optional).</param>
    /// <returns>An ArangoDB enumerable with the applied query options.</returns>
    public static IArangoDbEnumerable<TEntity> UsingOptions<TEntity>(
        this IArangoDbEnumerable<TEntity> enumerable,
        IQueryObject options,
        object argument = null)
    {
        if (options == null)
        {
            return enumerable;
        }

        return options switch
        {
            SimpleQueryObject simpleQuery => enumerable.UsingOptionsInternal(simpleQuery),
            QueryObjectList listObject => enumerable.UsingOptionsInternal(
                listObject,
                argument,
                AddObjectListToWhereClause),
            QueryObject genericFilterObject => enumerable.UsingOptionsInternal(
                genericFilterObject,
                argument,
                AddObjectGenericFilterToWhereClause),
            QueryObjectBase baseSettings => enumerable.UsingOptionsInternal(baseSettings),
            _ => throw new NotSupportedException(
                $"This type of {nameof(IQueryObject)} is not supported (current type: '{options.GetType()}').")
        };
    }

    internal static IArangoDbEnumerable<TEntity> CastAndResolveProperties<TEntity, TTarget>(
        this IArangoDbEnumerable<TEntity> enumerable,
        params Type[] supportedTypes)
    {
        // if supported types are empty, all types are supported
        if (supportedTypes is
            {
                Length: > 0
            }
            && !supportedTypes.Contains(typeof(TTarget)))
        {
            return enumerable;
        }

        var next = new ArangoDbEnumerable<TEntity>(enumerable.GetTypedEnumerable());

        next.ActivatedConversions.Add(typeof(TTarget));

        return next;
    }

    internal static IArangoDbEnumerable<TEntity> CastAndResolveProperties<TEntity, TTarget>(
        this IArangoDbSingleEnumerable<TEntity> enumerable)
    {
        var next = new ArangoDbEnumerable<TEntity>(enumerable.GetTypedEnumerable());

        next.ActivatedConversions.Add(typeof(TTarget));

        return next;
    }

    internal static IArangoDbEnumerable<TEntity> Combine<TEntity>(this IArangoDbEnumerable enumerable)
    {
        return new ArangoDbEnumerable<TEntity>(Guid.NewGuid().ToString(), enumerable, new CombinedQuerySettings());
    }

    internal static IArangoDbEnumerable<TEntity> Combine<TEntity>(
        this IArangoDbEnumerable enumerable,
        bool combinedByAnd)
    {
        return new ArangoDbEnumerable<TEntity>(
            Guid.NewGuid().ToString(),
            enumerable,
            new CombinedQuerySettings
            {
                CombinedByAnd = combinedByAnd
            });
    }

    internal static string ToQuery<TEntity>(
        this IArangoDbEnumerable<TEntity> enumerable,
        CollectionScope collectionScope)
    {
        return enumerable.Compile<TEntity>(collectionScope).GetQueryString();
    }

    /// <summary>
    ///     Creates a new ArangoDB enumerable with distinct elements based on a specified property.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TProp">The type of the property used for distinction.</typeparam>
    /// <param name="enumerable">The ArangoDB enumerable.</param>
    /// <param name="propertySelector">The property selector expression.</param>
    /// <returns>An ArangoDB enumerable with distinct elements based on the specified property.</returns>
    public static IArangoDbEnumerable<TEntity> DistinctByKey<TEntity, TProp>(
        this IArangoDbEnumerable<TEntity> enumerable,
        Expression<Func<TEntity, TProp>> propertySelector)
    {
        var next = new ArangoDbEnumerable<TEntity>(enumerable.GetTypedEnumerable());

        if (propertySelector?.Body is not MemberExpression memberExpression)
        {
            throw new ArgumentException("Wrong expression type!", nameof(propertySelector));
        }

        if (string.IsNullOrEmpty(memberExpression.Member.Name))
        {
            throw new ArgumentException("Cannot extract member name from expression.", nameof(propertySelector));
        }

        next.DistinctionKey = memberExpression;

        return next;
    }
}
