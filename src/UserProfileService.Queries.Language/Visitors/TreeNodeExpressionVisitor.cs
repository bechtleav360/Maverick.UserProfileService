using System.Linq.Expressions;
using UserProfileService.Queries.Language.ExpressionOperators;
using UserProfileService.Queries.Language.Helpers;
using UserProfileService.Queries.Language.TreeDefinition;

namespace UserProfileService.Queries.Language.Visitors;

/// <summary>
///     The tree node expression visitor is used to create out of the
///     the normal tree an lambda expression that is used to create where
///     query.
/// </summary>
public class TreeNodeExpressionVisitor : TreeNodeVisitorBase<Expression>
{
    /// <inheritdoc />
    protected override Expression VisitBinaryExpressionNode(
        BinaryExpressionNode expression,
        params object[] optionalParameter)
    {
        if (expression == null)
        {
            throw new ArgumentNullException(nameof(expression));
        }

        return expression.BinaryType switch
        {
            ExpressionCombinator.And => Expression.AndAlso(
                VisitNode(expression.LeftChild, optionalParameter),
                VisitNode(expression.RightChild, optionalParameter)),
            ExpressionCombinator.Or => Expression.OrElse(
                VisitNode(expression.LeftChild, optionalParameter),
                VisitNode(expression.RightChild, optionalParameter)),
            _ => throw new ArgumentOutOfRangeException(
                $"The binary type is not valid. The binary type has the value {expression}.")
        };
    }

    /// <inheritdoc />
    protected override Expression VisitExpressionNode(ExpressionNode? property, params object[] optionalParameter)
    {
        if (property == null)
        {
            throw new ArgumentNullException(nameof(property));
        }

        return ExpressionHelper.CreateSimpleExpression(
            property,
            optionalParameter.OfType<Type>().Single(),
            optionalParameter.OfType<ParameterExpression>().Single());
    }

    /// <inheritdoc />
    protected override Expression VisitRoot(RootNode root, params object[] optionalParameter)
    {
        if (root == null)
        {
            throw new ArgumentNullException(nameof(root));
        }

        Type type = optionalParameter.OfType<Type>().Single();

        ParameterExpression parameter = Expression.Parameter(type);

        Expression result = VisitNode(root, optionalParameter.Append(parameter).ToArray());

        return Expression.Lambda(typeof(Func<,>).MakeGenericType(type, typeof(bool)), result, parameter);
    }

    /// <inheritdoc />
    public override Expression<Func<TModel, bool>> Visit<TModel>(TreeNode? root)
    {
        if (root == null)
        {
            throw new ArgumentNullException(nameof(root));
        }

        if (root is not RootNode rootNode)
        {
            throw new ArgumentException($"Root not of type {nameof(RootNode)}.");
        }

        return (Expression<Func<TModel, bool>>)VisitRoot(rootNode, typeof(TModel));
    }
}
