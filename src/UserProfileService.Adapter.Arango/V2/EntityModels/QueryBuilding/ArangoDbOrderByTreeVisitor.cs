using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Maverick.UserProfileService.Models.EnumModels;
using UserProfileService.Adapter.Arango.V2.Contracts;

namespace UserProfileService.Adapter.Arango.V2.EntityModels.QueryBuilding;

/// <summary>
///     An ArangoDB tree visitor that processes order by expression trees.
/// </summary>
public sealed class ArangoDbOrderByTreeVisitor : ArangoDbTreeVisitorBase
{
    private ModelBuilderOptions _options;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ArangoDbOrderByTreeVisitor"/>.
    /// </summary>
    public ArangoDbOrderByTreeVisitor()
    {
        When<OrderByExpression>(VisitOrderByExpression);
    }

    private Expression VisitOrderByExpression(Expression node, VisitorMethodArgument argument)
    {
        if (node is not OrderByExpression oExpr)
        {
            throw new ArgumentException(
                $"The type of the argument 'node' is not supported by this method '{nameof(VisitOrderByExpression)}'.",
                nameof(node));
        }

        return Visit(oExpr.Body, OrderByExpressionArgument.CreateInstance(argument, oExpr.SortingOrder));
    }

    private static string ConvertToString(SortOrder sortingOrder)
    {
        return sortingOrder switch
        {
            SortOrder.Asc => "Asc",
            SortOrder.Desc => "DESC",
            _ => "Asc"
        };
    }

    /// <inheritdoc />
    protected override ModelBuilderOptions GetModelOptions()
    {
        return _options;
    }

    /// <inheritdoc />
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
        throw new InvalidOperationException("This operation is not supported by this tree visitor! Wrong linq syntax!");
    }

    /// <inheritdoc />
    protected override Expression VisitConstant(ConstantExpression node, VisitorMethodArgument argument)
    {
        if (Key == null)
        {
            throw new ArgumentOutOfRangeException(nameof(node), $"Missing or wrong key ('{Key}').");
        }

        if (node.Value is not string s)
        {
            throw new NotSupportedException(
                "This visitor only supports a constant expression that contains a string value.");
        }

        string sortOrder = argument is OrderByExpressionArgument oArg
            ? ConvertToString(oArg.SortingOrder)
            : ConvertToString(SortOrder.Asc);

        return new SubTreeVisitorResult
        {
            //HINT:  the expression key._key (id of entity) is added to sort expression to avoid random behavior for elements with same
            // sort property value (especially in cluster mode)
            ReturnString = $"SORT {Key}.{s} {sortOrder},{Key}._key",
            CollectionToIterationVarMapping =
                new Dictionary<string, string>(VarMapping, StringComparer.OrdinalIgnoreCase)
        };
    }

    /// <inheritdoc />
    protected override Expression VisitMember(MemberExpression node, VisitorMethodArgument argument)
    {
        if (Key == null)
        {
            throw new ArgumentOutOfRangeException(nameof(node), $"Missing or wrong key ('{Key}').");
        }

        string sortOrder = argument is OrderByExpressionArgument oArg
            ? ConvertToString(oArg.SortingOrder)
            : ConvertToString(SortOrder.Asc);

        return new SubTreeVisitorResult
        {
            //HINT:  the expression key._key (id of entity) is added to sort expression to avoid random behavior for elements with same
            // sort property value (especially in cluster mode)
            ReturnString = $"SORT {Key}.{node.Member.Name} {sortOrder},{Key}._key",
            CollectionToIterationVarMapping =
                new Dictionary<string, string>(VarMapping, StringComparer.OrdinalIgnoreCase)
        };
    }

    /// <summary>
    ///     Gets the result expression for an ArangoDB enumerable.
    /// </summary>
    /// <param name="enumerable">The ArangoDB enumerable.</param>
    /// <param name="collectionScope">The collection scope.</param>
    /// <param name="precedingVariableCollectionMapping">Mapping of preceding variable names to collection keys.</param>
    /// <returns>The result expression as a <see cref="SubTreeVisitorResult"/>.</returns>
    public SubTreeVisitorResult GetResultExpression(
        IArangoDbEnumerable enumerable,
        CollectionScope collectionScope,
        Dictionary<string, string> precedingVariableCollectionMapping)
    {
        Lock.Wait(1);
        _options = enumerable.GetEnumerable().ModelSettings;
        Scope = collectionScope;

        VarMapping = precedingVariableCollectionMapping == null
            ? new Dictionary<string, string>()
            : new Dictionary<string, string>(precedingVariableCollectionMapping);

        try
        {
            List<Expression> expressions = enumerable.GetEnumerable().OrderExpressions;

            if (expressions == null || !expressions.Any())
            {
                throw new Exception("Missing selection in linq query!");
            }

            Expression t = Visit(expressions.Last());

            return t as SubTreeVisitorResult
                ?? throw new Exception("Wrong return type in selection visitor class!");
        }
        finally
        {
            Lock.Release(1);
        }
    }

    /// <inheritdoc />
    public override SubTreeVisitorResult GetResultExpression(
        IArangoDbEnumerable enumerable,
        CollectionScope collectionScope)
    {
        return GetResultExpression(enumerable, collectionScope, null);
    }

    private class OrderByExpressionArgument : VisitorMethodArgument
    {
        internal SortOrder SortingOrder { get; }

        private OrderByExpressionArgument(VisitorMethodArgument old, SortOrder sortingOrder) : base(old)
        {
            SortingOrder = sortingOrder;
        }

        private OrderByExpressionArgument(SortOrder sortingOrder)
        {
            SortingOrder = sortingOrder;
        }

        internal static VisitorMethodArgument CreateInstance(VisitorMethodArgument old, SortOrder sortingOrder)
        {
            return old != null
                ? new OrderByExpressionArgument(old, sortingOrder)
                : new OrderByExpressionArgument(sortingOrder);
        }
    }
}
