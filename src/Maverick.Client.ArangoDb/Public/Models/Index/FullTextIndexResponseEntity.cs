namespace Maverick.Client.ArangoDb.Public.Models.Index;

/// <summary>
///     Object that has been returned by creating a FulltextIndex
/// </summary>
/// <inheritdoc />
public class FullTextIndexResponseEntity : IndexResponseEntity
{
    /// <summary>
    ///     Minimum character length of words to index
    /// </summary>
    public int MinLength { get; set; }
}
