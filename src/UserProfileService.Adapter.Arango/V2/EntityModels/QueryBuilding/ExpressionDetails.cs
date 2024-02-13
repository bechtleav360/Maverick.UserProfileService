using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace UserProfileService.Adapter.Arango.V2.EntityModels.QueryBuilding;

/// <summary>
///     Represents details about an expression.
/// </summary>
public class ExpressionDetails
{
    internal string ExpressionId { get; }
    internal bool IsGeneric { get; }
    internal Expression UsedExpression { get; }

    /// <summary>
    ///     Gets or sets a list of batched lambda expressions.
    /// </summary>
    public List<LambdaExpression> BatchedExpressions { get; } = new List<LambdaExpression>();
    /// <summary>
    ///     Gets or sets a value indicating whether the expressions are combined using "AND" logic.
    /// </summary>
    public bool? CombinedByAnd { get; set; }
    /// <summary>
    ///     Gets or sets information about nested properties.
    /// </summary>
    public IList<NestedPropertyInformation> NestedPropertyInformation { get; set; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ExpressionDetails"/> class with the specified ID.
    /// </summary>
    /// <param name="id">The expression ID.</param>
    public ExpressionDetails(string id)
    {
        ExpressionId = id;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ExpressionDetails"/> class with the specified ID and batched expressions.
    /// </summary>
    /// <param name="id">The expression ID.</param>
    /// <param name="batchedExpressions">The batched lambda expressions.</param>
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

    /// <summary>
    ///     Initializes a new instance of the <see cref="ExpressionDetails"/> class with the specified ID and expression.
    /// </summary>
    /// <param name="id">The expression ID.</param>
    /// <param name="expression">The used expression.</param>
    public ExpressionDetails(
        string id,
        Expression expression) : this(id, expression, false)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ExpressionDetails"/> class with the specified ID, expression, and generic flag.
    /// </summary>
    /// <param name="id">The expression ID.</param>
    /// <param name="expression">The used expression.</param>
    /// <param name="isGeneric">Indicates whether the expression is generic.</param>
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
