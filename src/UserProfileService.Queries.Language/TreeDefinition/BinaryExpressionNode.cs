using UserProfileService.Queries.Language.ExpressionOperators;

namespace UserProfileService.Queries.Language.TreeDefinition;

/// <summary>
///     The binary expression node combine two <see cref="TreeNode" />
///     with a <see cref="BinaryType" />. The binary operators can be an 'or'
///     or an 'and'.
/// </summary>
public class BinaryExpressionNode : TreeNodeWithTwoChildren
{
    /// <summary>
    ///     The binary operator that combines the two <see cref="ExpressionNode" />.
    /// </summary>
    public ExpressionCombinator BinaryType { get; set; }

    /// <summary>
    ///     Creates a <see cref="BinaryExpressionNode" />.
    /// </summary>
    /// <param name="parent">The parent of the <see cref="BinaryExpressionNode" />.</param>
    /// <param name="id">The id of the <see cref="BinaryExpressionNode" />.</param>
    protected BinaryExpressionNode(TreeNode? parent, string id)
        : base(parent, id)
    {
    }

    /// <summary>
    ///     Creates a <see cref="BinaryExpressionNode" /> object.
    /// </summary>
    /// <param name="left">The left <see cref="TreeNode" /> that is always an <see cref="ExpressionNode" />.</param>
    /// <param name="right">
    ///     The right <see cref="TreeNode" /> that is or an other <see cref="BinaryExpressionNode" /> or
    ///     <see cref="ExpressionNode" />.
    /// </param>
    /// <param name="op">The operator that combines the two <see cref="TreeNode" />.</param>
    public BinaryExpressionNode(TreeNode left, TreeNode right, ExpressionCombinator op) : base(left, right)
    {
        BinaryType = op;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{BinaryType}";
    }
}
