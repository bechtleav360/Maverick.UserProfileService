namespace Maverick.Client.ArangoDb.Public.Models.Document;

/// <summary>
///     Base model for POST document responses.
/// </summary>
public class DocumentBase
{
    /// <summary>
    ///     ArangoDB document ID.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// ArangoDB document key.
    /// </summary>
    public string Key { get; set; }

    /// <summary>
    ///     ArangoDB document revision tag.
    /// </summary>
    public string Rev { get; set; }
}
