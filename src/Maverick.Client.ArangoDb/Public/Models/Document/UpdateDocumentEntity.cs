using Newtonsoft.Json;

namespace Maverick.Client.ArangoDb.Public.Models.Document;

/// <summary>
///     Contains some attributes of the old (before update) and the new document.
/// </summary>
/// <typeparam name="T"></typeparam>
public class UpdateDocumentEntity<T>
{
    /// <summary>
    ///     Document id.
    /// </summary>
    [JsonProperty("_id")]
    public string Id { get; set; }

    /// <summary>
    ///     ArangoDB document key.
    /// </summary>
    [JsonProperty("_key")]
    public string Key { get; set; }

    /// <summary>
    ///     new document
    /// </summary>
    public T New { get; set; }

    /// <summary>
    ///     old document
    /// </summary>
    public T Old { get; set; }

    /// <summary>
    ///     old revision tag.
    /// </summary>
    [JsonProperty("_oldRev")]
    public string OldRev { get; set; }

    /// <summary>
    ///     ArangoDB revision tag.
    /// </summary>
    [JsonProperty("_rev")]
    public string Rev { get; set; }
}
