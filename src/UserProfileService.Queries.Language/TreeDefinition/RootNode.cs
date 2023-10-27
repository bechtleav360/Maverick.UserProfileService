using UserProfileService.Queries.Language.ExpressionOperators;

namespace UserProfileService.Queries.Language.TreeDefinition;

/// <summary>
///     The root node of the tree that can be used to travers the
///     tree.
/// </summary>
public class RootNode : TreeNodeWithTwoChildren
{
    /// <summary>
    ///     The operation type that the tree is used for.
    /// </summary>
    public FilterOption OperatorType;

    /// <summary>
    ///     The original query string for the operator type.
    ///     That should be used for debug purposes.
    /// </summary>
    public string OriginalString;

    /// <summary>
    ///     Creates a <see cref="RootNode" /> object.
    /// </summary>
    /// <param name="operator">The operation type that the tree is used for.</param>
    /// <param name="originalQuery">
    ///     The original query string for the operator type.
    ///     That should be used for debug purposes.
    /// </param>
    /// <param name="parent">The parent of the <see cref="RootNode" />.</param>
    public RootNode(
        FilterOption @operator,
        string originalQuery,
        TreeNode? parent = null) : base(parent)
    {
        OperatorType = @operator;
        OriginalString = originalQuery;
    }
}
