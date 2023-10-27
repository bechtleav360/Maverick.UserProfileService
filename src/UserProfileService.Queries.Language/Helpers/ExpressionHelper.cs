using System.Linq.Expressions;
using System.Reflection;
using UserProfileService.Queries.Language.ExpressionOperators;
using UserProfileService.Queries.Language.TreeDefinition;
using UserProfileService.Queries.Language.ValidationException;

namespace UserProfileService.Queries.Language.Helpers;

/// <summary>
///     The helper is there fore to create a valid <see cref="Expression" /> out of
///     an expression node.
/// </summary>
internal static class ExpressionHelper
{
    private static Expression CreateContainsExpression(
        MemberExpression memberExpression,
        ConstantExpression constExp)
    {
        if (memberExpression == null)
        {
            throw new ArgumentNullException(nameof(memberExpression));
        }

        if (constExp == null)
        {
            throw new ArgumentNullException(nameof(constExp));
        }

        MethodInfo? containsMethode = typeof(string).GetMethod(
            nameof(string.Contains),
            BindingFlags.Public | BindingFlags.Instance,
            null,
            new[] { typeof(string), typeof(StringComparison) },
            null);

        if (containsMethode == null)
        {
            throw new ArgumentNullException(nameof(containsMethode));
        }

        return Expression.Call(
            memberExpression,
            containsMethode,
            constExp,
            Expression.Constant(StringComparison.OrdinalIgnoreCase));
    }

    internal static Expression CreateSimpleExpression(
        ExpressionNode expressionNode,
        Type resultObject,
        ParameterExpression parameter)
    {
        if (expressionNode == null)
        {
            throw new ArgumentNullException(nameof(expressionNode));
        }

        if (resultObject == null)
        {
            throw new ArgumentNullException(nameof(resultObject));
        }

        if (parameter == null)
        {
            throw new ArgumentNullException(nameof(parameter));
        }

        PropertyInfo? propertyInfo = resultObject.GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .FirstOrDefault(
                p => p.Name.Equals(
                    expressionNode.LeftSideExpression,
                    StringComparison.OrdinalIgnoreCase));

        if (propertyInfo == null)
        {
            throw new QueryValidationException(
                "The $filter query is not valid!",
                $"The property '{expressionNode.LeftSideExpression}' is not part of the object '{resultObject.Name}'");
        }

        MemberExpression propertyExpression = Expression.Property(parameter, propertyInfo);

        object parsedValue = DataTypeParserHelper.ParseDataType(
            propertyInfo.PropertyType,
            expressionNode.RightSideExpression,
            expressionNode.LeftSideExpression);

        ConstantExpression constantExpression = Expression.Constant(parsedValue);

        return expressionNode.Operator switch
        {
            OperatorType.EqualsOperator =>
                Expression.Equal(propertyExpression, constantExpression),

            OperatorType.NotEqualsOperator =>
                Expression.NotEqual(propertyExpression, constantExpression),

            OperatorType.GreaterThenOperator =>
                Expression.GreaterThan(
                    propertyExpression,
                    constantExpression),

            OperatorType.GreaterEqualsOperator =>
                Expression.GreaterThanOrEqual(
                    propertyExpression,
                    constantExpression),

            OperatorType.LessThenOperator =>
                Expression.LessThan(
                    propertyExpression,
                    constantExpression),

            OperatorType.LessEqualsOperator =>
                Expression.LessThanOrEqual(
                    propertyExpression,
                    constantExpression),

            OperatorType.Contains => CreateContainsExpression(
                propertyExpression,
                constantExpression),

            _ => throw new ArgumentOutOfRangeException($"The operator {expressionNode.Operator} is not supported.")
        };
    }
}
