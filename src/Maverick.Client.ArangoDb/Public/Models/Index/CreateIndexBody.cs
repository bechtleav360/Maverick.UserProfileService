namespace Maverick.Client.ArangoDb.Public.Models.Index;

/// <summary>
///     Contains some attributes that can be setted to create an index.
/// </summary>
public class CreateIndexBody
{
    /// <summary>
    ///     if false, the deduplication of array values is turned off.
    /// </summary>
    public bool? Deduplicate { get; set; }

    /// <summary>
    ///     The time (in seconds) after a document's creation after which the
    ///     documents count as "expired".
    /// </summary>
    public int? ExpireAfter { get; set; }

    /// <summary>
    ///     an array of attribute names
    /// </summary>
    public string[] Fields { get; set; }

    /// <summary>
    ///     If a geo-spatial index on a location is constructed
    ///     and geoJson is true, then the order within the array is longitude
    ///     followed by latitude.This corresponds to the format described in
    ///     http://geojson.org/geojson-spec.html#positions
    /// </summary>
    public bool? GeoJson { get; set; }

    /// <summary>
    ///     Minimum character length of words to index. Will default
    ///     to a server-defined value if unspecified.It is thus recommended to set
    ///     this value explicitly when creating the index.
    /// </summary>
    public int? MinLength { get; set; }

    /// <summary>
    ///     if true, then create a sparse index.
    /// </summary>
    public bool? Sparse { get; set; }

    /// <summary>
    ///     Index Type
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    ///     if true, then create a unique index.
    /// </summary>
    public bool? Unique { get; set; }
}
