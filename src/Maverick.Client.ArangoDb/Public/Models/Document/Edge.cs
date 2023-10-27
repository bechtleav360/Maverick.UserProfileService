using Newtonsoft.Json;

namespace Maverick.Client.ArangoDb.Public.Models.Document;

/// <summary>
///     Contains edge common attributes
/// </summary>
/// <inheritdoc />
public class Edge : DocumentBase
{
    /// <summary>
    ///     document handle of the linked vertex (incoming relation)
    /// </summary>
    [JsonProperty("_from")]
    public string From { get; set; }

    /// <summary>
    ///     Edge label
    /// </summary>
    public string Label { get; set; }

    /// <summary>
    ///     document handle of the linked vertex (outgoing relation)
    /// </summary>
    [JsonProperty("_to")]
    public string To { get; set; }
}
