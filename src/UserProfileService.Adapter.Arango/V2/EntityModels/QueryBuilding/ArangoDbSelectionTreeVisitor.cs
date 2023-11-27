using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Maverick.Client.ArangoDb.Public;
using Newtonsoft.Json;
using UserProfileService.Adapter.Arango.V2.Abstractions;
using UserProfileService.Adapter.Arango.V2.Contracts;

namespace UserProfileService.Adapter.Arango.V2.EntityModels.QueryBuilding;

public sealed class ArangoDbSelectionTreeVisitor : ArangoDbTreeVisitorBase
{
    private ModelBuilderOptions _options;

    private bool TryResolve(Type entityType, Type targetType, out SubTreeVisitorResult result)
    {
        IList<TypePropertyType> mapperInfo = _options.GetPropertyMapperInformation(entityType, targetType);

        if (mapperInfo == null || !VarMapping.TryGetValue(GetCollectionName(entityType, Scope), out string key))
        {
            result = null;

            return false;
        }

        var sb = new StringBuilder($"RETURN MERGE({key},{{");
        var loop = 0;

        foreach (TypePropertyType info in mapperInfo)
        {
            var mapper = (IEntityPropertyAqlMapper)Activator.CreateInstance(info.MapperType);

            if (mapper == null)
            {
                throw new Exception($"No AQL mapper found for type '{info.MapperType.Name}'.");
            }

            if (loop++ > 0)
            {
                sb.Append(",");
            }

            sb.Append($"\"{info.PropertyToBeMappedTo.Name}\":(");
            sb.Append($"FOR resolvedPropertyMapping IN NOT_NULL({key}.{info.PropertyToBeMappedFrom.Name},[])");
            sb.Append($"FOR previousObject IN {GetCollectionName(info.ResolvingType, Scope)} ");
            sb.Append($"FILTER resolvedPropertyMapping==previousObject.{AConstants.IdSystemProperty} ");
            sb.Append("RETURN ");
            sb.Append(mapper.GetConvertingAqlQuery("previousObject"));
            sb.Append(")");
        }

        sb.Append("})");

        result = new SubTreeVisitorResult
        {
            CollectionToIterationVarMapping = new Dictionary<string, string>(VarMapping),
            ReturnString = sb.ToString()
        };

        return true;
    }

    private static string GetJsonPropertyName(MemberInfo memberInfo)
    {
        return memberInfo?.GetCustomAttributes(typeof(JsonPropertyAttribute), false).FirstOrDefault() is
            JsonPropertyAttribute jsonProperty
            ? jsonProperty.PropertyName
            : null;
    }

    /// <inheritdoc />
    protected override ModelBuilderOptions GetModelOptions()
    {
        return _options;
    }

    protected override Expression VisitLambda<T>(Expression<T> node, VisitorMethodArgument argument)
    {
        if (!TryResolveAndUpdateKey(node))
        {
            return node;
        }

        Expression result = Visit(node.Body);

        Key = null;

        return result ?? Expression.Empty();
    }

    /// <inheritdoc />
    protected override Expression VisitParameter(ParameterExpression node, VisitorMethodArgument argument)
    {
        if (Key == null)
        {
            throw new ArgumentOutOfRangeException(nameof(node), $"Missing or wrong key ('{Key}').");
        }

        return new SubTreeVisitorResult
        {
            ReturnString = $"RETURN {Key}",
            CollectionToIterationVarMapping = VarMapping
        };
    }

    /// <inheritdoc />
    protected override Expression VisitConstant(ConstantExpression node, VisitorMethodArgument argument)
    {
        throw new InvalidOperationException("This operation is not supported by this tree visitor! Wrong linq syntax!");
    }

    protected override Expression VisitMember(MemberExpression node)
    {
        if (Key == null)
        {
            throw new ArgumentOutOfRangeException(nameof(node), $"Missing or wrong key ('{Key}').");
        }

        return new SubTreeVisitorResult
        {
            ReturnString = $"RETURN {Key}.{GetJsonPropertyName(node.Member) ?? node.Member.Name}",
            CollectionToIterationVarMapping =
                new Dictionary<string, string>(VarMapping, StringComparer.OrdinalIgnoreCase)
        };
    }

    public override SubTreeVisitorResult GetResultExpression(
        IArangoDbEnumerable enumerable,
        CollectionScope collectionScope)
    {
        return GetResultExpression(enumerable, collectionScope, null);
    }

    public SubTreeVisitorResult GetResultExpression(
        IArangoDbEnumerable enumerable,
        CollectionScope collectionScope,
        Dictionary<string, string> precedingVariableCollectionMapping)
    {
        Lock.Wait(1);
        Scope = collectionScope;
        _options = enumerable.GetEnumerable().ModelSettings;

        VarMapping = precedingVariableCollectionMapping == null
            ? new Dictionary<string, string>()
            : new Dictionary<string, string>(precedingVariableCollectionMapping);

        try
        {
            ArangoDbEnumerable enumerableObject = enumerable.GetEnumerable();

            List<Expression> expressions = enumerableObject.SelectExpressions;

            if (expressions == null || !expressions.Any())
            {
                throw new Exception("Missing selection in linq query!");
            }

            Expression t = Visit(expressions.Last());

            if (enumerableObject.ActivatedConversions.Count > 0
                && TryResolve(
                    enumerableObject.GetInnerType(),
                    enumerableObject.ActivatedConversions.First(),
                    out SubTreeVisitorResult resolvingResult))
            {
                return resolvingResult;
            }

            return t as SubTreeVisitorResult
                ?? throw new Exception("Wrong return type in selection visitor class!");
        }
        finally
        {
            Lock.Release(1);
        }
    }
}
