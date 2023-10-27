namespace Maverick.Client.ArangoDb.Public.Models.Index;

/// <summary>
///     Contains all informations about a speficied index
/// </summary>
/// <inheritdoc />
public class IndexResponseWithSelectivityEntity : IndexResponseEntity
{
    /// <summary>
    ///     determines how many documents will be returned by the index on average.
    /// </summary>
    public int SelectivityEstimate { get; set; }
}
