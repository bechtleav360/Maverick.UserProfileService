using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace UserProfileService.Adapter.Arango.V2.EntityModels.QueryBuilding;

public class ExpressionDetails
{
    internal string ExpressionId { get; }
    internal bool IsGeneric { get; }
    internal Expression UsedExpression { get; }
    public List<LambdaExpression> BatchedExpressions { get; } = new List<LambdaExpression>();
    public bool? CombinedByAnd { get; set; }
    public IList<NestedPropertyInformation> NestedPropertyInformation { get; set; }

    public ExpressionDetails(string id)
    {
        ExpressionId = id;
    }

    public ExpressionDetails(
        string id,
        IEnumerable<LambdaExpression> batchedExpressions)
    {
        if (batchedExpressions == null)
        {
            throw new ArgumentNullException(nameof(batchedExpressions));
        }

        ExpressionId = id;
        BatchedExpressions = batchedExpressions as List<LambdaExpression> ?? batchedExpressions.ToList();
    }

    public ExpressionDetails(
        string id,
        Expression expression) : this(id, expression, false)
    {
    }

    public ExpressionDetails(
        string id,
        Expression expression,
        bool isGeneric)
    {
        UsedExpression = expression;
        IsGeneric = isGeneric;
        ExpressionId = id;
    }
}
