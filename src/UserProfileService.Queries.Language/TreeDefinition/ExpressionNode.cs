using UserProfileService.Queries.Language.ExpressionOperators;

namespace UserProfileService.Queries.Language.TreeDefinition;

/// <summary>
///     The <see cref="ExpressionNode" /> combines the left hand side with the
///     right hand side with an <see cref="Operator" />.
/// </summary>
public class ExpressionNode : TreeNode
{
    /// <summary>
    ///     The left hand side expression that is normally the name of a property
    ///     of an object.
    /// </summary>
    public string LeftSideExpression { get; set; }

    /// <summary>
    ///     The operator that combines the left side expression.
    /// </summary>
    public OperatorType Operator { get; set; }

    /// <summary>
    ///     The right hand side expression that is normally a value
    ///     that should be compared with the left side expression.
    /// </summary>
    public string RightSideExpression { get; set; }

    /// <summary>
    ///     Creates an <see cref="ExpressionNode" /> object.
    /// </summary>
    /// <param name="parent">The parent of the <see cref="ExpressionNode" />.</param>
    /// <param name="id">The id of the <see cref="ExpressionNode" />.</param>
    /// <param name="leftSideExpression">
    ///     The left hand side expression that is normally the name of a property
    ///     of an object.
    /// </param>
    /// <param name="rightSideExpression">
    ///     The right hand side expression that is normally a value
    ///     that should be compared with the left side expression.
    /// </param>
    /// <param name="operatorType"> The operator that combines the left side expression.</param>
    protected ExpressionNode(
        OperatorType operatorType,
        TreeNode? parent,
        string id,
        string leftSideExpression,
        string rightSideExpression)
        : base(parent, id)
    {
        LeftSideExpression = leftSideExpression;
        RightSideExpression = rightSideExpression;
        Operator = operatorType;
    }

    /// <summary>
    ///     Creates an <see cref="ExpressionNode" /> object.
    /// </summary>
    /// <param name="parent">The parent of the <see cref="ExpressionNode" />.</param>
    /// <param name="operatorType">   The operator that combines the left side expression.</param>
    /// <param name="leftSideExpression">
    ///     The left hand side expression that is normally the name of a property
    ///     of an object.
    /// </param>
    /// <param name="rightSideExpression">
    ///     The right hand side expression that is normally a value
    ///     that should be compared with the left side expression.
    /// </param>
    public ExpressionNode(
        TreeNode? parent,
        OperatorType operatorType,
        string leftSideExpression,
        string rightSideExpression)
        : base(parent)
    {
        LeftSideExpression = leftSideExpression;
        RightSideExpression = rightSideExpression;
        Operator = operatorType;
    }

    /// <summary>
    ///     Creates an <see cref="ExpressionNode" /> object.
    /// </summary>
    /// <param name="leftSideExpression">
    ///     The left hand side expression that is normally the name of a property
    ///     of an object.
    /// </param>
    /// <param name="rightSideExpression">
    ///     The right hand side expression that is normally a value
    ///     that should be compared with the left side expression.
    /// </param>
    /// <param name="operator"> The operator that combines the left side expression.</param>
    public ExpressionNode(
        string leftSideExpression,
        string rightSideExpression,
        OperatorType @operator) : base(null)
    {
        LeftSideExpression = leftSideExpression;
        RightSideExpression = rightSideExpression;
        Operator = @operator;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return string.Concat(LeftSideExpression, Operator, RightSideExpression);
    }
}
