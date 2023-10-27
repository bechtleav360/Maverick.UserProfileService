namespace UserProfileService.Queries.Language.TreeDefinition;

/// <summary>
///     The tree node that derived from <see cref="TreeNode" />
///     and contains two children. A left and a right child.
/// </summary>
public class TreeNodeWithTwoChildren : TreeNode
{
    /// <summary>
    ///     The left child of the node.
    /// </summary>
    public TreeNode LeftChild { set; get; } = null!;

    /// <summary>
    ///     The right child of the node.
    /// </summary>
    public TreeNode RightChild { get; set; } = null!;

    /// <summary>
    ///     Creates a <see cref="TreeNodeWithTwoChildren" /> object.
    /// </summary>
    /// <param name="parent">The parent of the node.</param>
    public TreeNodeWithTwoChildren(TreeNode? parent) : base(parent)
    {
    }

    /// <summary>
    ///     Creates a <see cref="TreeNodeWithTwoChildren" /> object.
    /// </summary>
    /// <param name="left">The left child of the node.</param>
    /// <param name="right">The right child of the node.</param>
    public TreeNodeWithTwoChildren(TreeNode left, TreeNode right) : base(null)
    {
        LeftChild = left;
        RightChild = right;
    }

    /// <summary>
    ///     Creates a <see cref="TreeNodeWithTwoChildren" /> object.
    /// </summary>
    /// <param name="parent">The parent of the node</param>
    /// <param name="id">The id of the node.</param>
    public TreeNodeWithTwoChildren(TreeNode? parent, string id) : base(parent, id)
    {
    }
}
