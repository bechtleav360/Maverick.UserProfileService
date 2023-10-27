namespace Maverick.Client.ArangoDb.Public.Models.Collection;

/// <summary>
///     Object contains collection properties, the revision id and the calculated checksum
/// </summary>
/// <inheritdoc />
public class CollectionWithCheckSumAndRevisionIdEntity : CollectionWithRevisionEntity
{
    /// <summary>
    ///     The calculated checksum
    /// </summary>
    public string CheckSum { get; set; }
}
