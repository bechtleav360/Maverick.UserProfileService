namespace UserProfileService.Queries.Language.TreeDefinition;

/// <summary>
///     An abstract tree node from whom all other nodes are derived from.
/// </summary>
public abstract class TreeNode
{
    /// <summary>
    ///     The id of the node.
    /// </summary>
    public string Id { get; protected set; }

    /// <summary>
    ///     The root node of the treeNode.
    /// </summary>
    public TreeNode? Root { get; protected set; }

    /// <summary>
    ///     The type name of the node.
    /// </summary>
    public string Type => GetType().Name;

    /// <summary>
    ///     Creates a <see cref="TreeNode" /> object.
    /// </summary>
    /// <param name="parent"></param>
    protected TreeNode(TreeNode? parent)
        : this(parent, Guid.NewGuid().ToString("D"))
    {
    }

    /// <summary>
    ///     Creates a <see cref="TreeNode" /> object.
    /// </summary>
    /// <param name="parent">The parent of the tree node.</param>
    /// <param name="id">The id of the node.</param>
    protected TreeNode(TreeNode? parent, string id)
    {
        Root = parent?.Root;
        Id = id;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return Type;
    }
}
