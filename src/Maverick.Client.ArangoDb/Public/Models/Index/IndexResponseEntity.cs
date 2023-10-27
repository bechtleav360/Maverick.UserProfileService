namespace Maverick.Client.ArangoDb.Public.Models.Index;

/// <summary>
///     contains common index information
/// </summary>
public class IndexResponseEntity
{
    /// <summary>
    ///     an array of attribute paths.
    /// </summary>
    public string[] Fields { get; set; }

    /// <summary>
    ///     Index id
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    ///     is true if the index has been newly created
    /// </summary>
    public bool IsNewlyCreated { get; set; }

    /// <summary>
    ///     The name of the index
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     Sparse means that documents that do not contain
    ///     the index attributes or have non-numeric values in the index attributes
    ///     will not be indexed.
    /// </summary>
    public bool Sparse { get; set; }

    /// <summary>
    ///     Index type
    /// </summary>
    public AIndexType Type { get; set; }

    /// <summary>
    ///     Some indexes can be created as unique or non-unique variants. Uniqueness
    ///     can be controlled for most indexes by specifying the unique flag in the
    ///     index details.Setting it to true will create a unique index.
    ///     Setting it to false or omitting the unique attribute will
    ///     create a non-unique index.
    /// </summary>
    public bool Unique { get; set; }
}
