using System;
using System.Data;
using System.Linq;
using System.Linq.Expressions;

namespace UserProfileService.Messaging.ArangoDb.Extensions;

/// <summary>
///     Contains extension methods related to <see cref="Expression{T}" />s and the corresponding implementations.
/// </summary>
public static class ExpressionExtension
{
    /// <summary>
    ///     Extracts the name of the expression including the concatenation.
    ///     (o => o.Object.Name, null) => "o.Object.Name" (excl. <paramref name="baseParameter" />)
    ///     (o => o.Object.Name, "x" ) => "x.Object.Name" (incl. <paramref name="baseParameter" />)
    /// </summary>
    /// <typeparam name="TModel">Model to get the expression based on.</typeparam>
    /// <typeparam name="TValue">Value of expression.</typeparam>
    /// <param name="expression">Expression to get name from.</param>
    /// <param name="baseParameter">The parameter to be used instead of the first parameter of the expression.</param>
    /// <returns>Name of expression.</returns>
    /// <exception cref="ArgumentException">
    ///     Will be thrown if <paramref name="expression"/> does not contain any parameters<br/>
    ///     -or-<br/>
    ///     If the first parameter of <paramref name="expression"/> is not equal to the first part
    ///     of the expression string.
    /// </exception>
    public static string GetName<TModel, TValue>(
        this Expression<Func<TModel, TValue>> expression,
        string baseParameter = null)
    {
        // First parameter will be replaced with baseParameter
        string firstParameter = expression.Parameters.FirstOrDefault()?.Name;

        if (string.IsNullOrEmpty(firstParameter))
        {
            throw new ArgumentException("Provided argument 'expression' has no parameter.", nameof(expression));
        }

        if (expression.Body is not MemberExpression body)
        {
            var unaryBody = (UnaryExpression)expression.Body;
            body = unaryBody.Operand as MemberExpression;
        }

        if (body == null)
        {
            throw new InvalidExpressionException("Could not cast body of expression to 'MemberExpression'");
        }

        var expressionString = body.ToString();

        if (string.IsNullOrWhiteSpace(baseParameter))
        {
            return expressionString;
        }

        if (expressionString.StartsWith(firstParameter))
        {
            return baseParameter + expressionString[firstParameter.Length..];
        }

        throw new ArgumentException(
            "The first parameter of expression is not equal to the first part of the expression string.",
            nameof(expression));
    }

    /// <summary>
    ///     Extracts the name of the filter expression
    /// </summary>
    /// <typeparam name="TModel">Model to get the expression based on.</typeparam>
    /// <param name="filterExpression"> Filter expression to get name from.</param>
    /// <param name="baseParameter">The parameter to be used instead of the first parameter of the filter expression.</param>
    /// <returns>Name of expression.</returns>
    /// <exception cref="ArgumentNullException">Will be thrown if the filter expression is null</exception>
    /// <exception cref="ArgumentException">
    ///     Will be thrown if the <paramref name="filterExpression"/> does not have any parameters<br/>
    ///     -or-<br/>
    ///     If The first parameter of <paramref name="filterExpression"/> is not equal to the first part
    ///     of the expression string.
    /// </exception>
    public static string GetName<TModel>(
        this Expression<Predicate<TModel>> filterExpression,
        string baseParameter = null)
    {
        if (filterExpression == null)
        {
            throw new ArgumentNullException(nameof(filterExpression));
        }

        string firstParameter = filterExpression.Parameters.FirstOrDefault()?.Name;

        if (string.IsNullOrEmpty(firstParameter))
        {
            throw new ArgumentException("Provided argument 'filterExpression' has no parameter.",
                nameof(filterExpression));
        }

        // remove ( and ) from expression string
        string expressionString = filterExpression.Body.ToString().Trim('(').Trim(')');


        if (expressionString.StartsWith(firstParameter))
        {
            return baseParameter + expressionString[firstParameter.Length..];
        }

        throw new ArgumentException(
            "The first parameter of expression is not equal to the first part of the expression string.",
            nameof(filterExpression));
    }
}
