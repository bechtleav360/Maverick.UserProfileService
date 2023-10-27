using Newtonsoft.Json;

namespace Maverick.Client.ArangoDb.Public.Models.Index;

/// <summary>
///     Coontains object that has been returned by creating a GeoIndex
/// </summary>
/// <inheritdoc />
public class GeoIndexResponseEntity : IndexResponseEntity
{
    /// <summary>
    ///     If a geo-spatial index on a location is constructed
    ///     and geoJson is true, then the order within the array is longitude
    ///     followed by latitude.This corresponds to the format described in http://geojson.org/geojson-spec.html#positions
    /// </summary>
    [JsonProperty("geoJson")]
    public bool IsGeoJson { get; set; }

    /// <summary>
    ///     worst indexed level
    /// </summary>
    public int WorstIndexedLevel { get; set; }
}
