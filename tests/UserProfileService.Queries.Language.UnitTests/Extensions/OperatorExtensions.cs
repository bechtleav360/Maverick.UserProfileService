using System;
using UserProfileService.Queries.Language.ExpressionOperators;

namespace UserProfileService.Queries.Language.Tests.Extensions;

public static class OperatorExtensions
{
    public static OperatorType GetOperatorType(this string @operator)
    {
        if (string.IsNullOrWhiteSpace(@operator))
        {
            throw new ArgumentException(null, nameof(@operator));
        }

        return @operator.ToLower() switch
               {
                   "le" => OperatorType.LessEqualsOperator,
                   "lt" => OperatorType.LessThenOperator,
                   "eq" => OperatorType.EqualsOperator,
                   "ne" => OperatorType.NotEqualsOperator,
                   "gt" => OperatorType.GreaterThenOperator,
                   "ge" => OperatorType.GreaterEqualsOperator,
                   _ => throw new ArgumentOutOfRangeException(
                       nameof(@operator),
                       $"Not expected direction value: {@operator}")
               };
    }

    public static ExpressionCombinator GetExpressionCombinator(this string @operator)
    {
        if (string.IsNullOrWhiteSpace(@operator))
        {
            throw new ArgumentException(null, nameof(@operator));
        }

        return @operator.ToLower() switch
               {
                   FilterExpressionCombinator.OrCombinator => ExpressionCombinator.Or,
                   FilterExpressionCombinator.AndCombinator => ExpressionCombinator.And,
                   _ => throw new ArgumentOutOfRangeException(nameof(@operator), $"Not expected direction value: {@operator}")
               };
    }
}
