using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Maverick.UserProfileService.Models.EnumModels;

namespace UserProfileService.Adapter.Arango.V2.EntityModels.QueryBuilding;

internal class NestedExpressionDetails : ExpressionDetails
{
    internal BinaryOperator BinaryOperator { get; set; } = BinaryOperator.Or;
    internal List<ExpressionDetails> Children { get; }

    public NestedExpressionDetails(
        string id,
        IEnumerable<ExpressionDetails> children)
        : base(id)
    {
        Children = children as List<ExpressionDetails> ?? children?.ToList();
    }

    public NestedExpressionDetails(
        string id,
        IEnumerable<LambdaExpression> batchedExpressions,
        IEnumerable<ExpressionDetails> children)
        : base(id, batchedExpressions)
    {
        Children = children as List<ExpressionDetails> ?? children?.ToList();
    }

    public NestedExpressionDetails(
        string id,
        Expression expression,
        IEnumerable<ExpressionDetails> children)
        : base(id, expression)
    {
        Children = children as List<ExpressionDetails> ?? children?.ToList();
    }

    public NestedExpressionDetails(
        string id,
        Expression expression,
        bool isGeneric,
        IEnumerable<ExpressionDetails> children)
        : base(id, expression, isGeneric)
    {
        Children = children as List<ExpressionDetails> ?? children?.ToList();
    }
}
