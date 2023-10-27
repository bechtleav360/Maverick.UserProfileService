namespace Maverick.Client.ArangoDb.Public.Models.Collection;

/// <summary>
///     This in an object describing the index
/// </summary>
public class CollectionIndex
{
    /// <summary>
    ///     an array of attribute paths
    /// </summary>
    public string[] Fields { get; set; }

    /// <summary>
    ///     The identifier of the index
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    ///     Index name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     It determines how many documents will be returned by the index on average
    /// </summary>
    public int SelectivityEstimate { get; set; }

    /// <summary>
    ///     is true, if the index is a sparse index. In a sparse index all documents will be excluded from the index that do
    ///     not
    ///     contain at least one of the specified index attributes(i.e.fields) or that
    ///     have a value of null in any of the specified index attributes.
    /// </summary>
    public bool Sparse { get; set; }

    /// <summary>
    ///     The index type
    /// </summary>
    public AIndexType Type { get; set; }

    /// <summary>
    ///     bool value that takes the value true if the index is unique
    /// </summary>
    public bool Unique { get; set; }
}
