using System.Collections.Generic;
using Newtonsoft.Json;

namespace Maverick.Client.ArangoDb.Public.Models.Document;

/// <summary>
///     Contains the common attributes of a document
/// </summary>
public class DocumentResponseEntity
{
    /// <summary>
    ///     contains the document id
    /// </summary>
    [JsonProperty("_id")]
    public string Id { get; set; }

    /// <summary>
    ///     contains the document key
    /// </summary>
    [JsonProperty("_key")]
    public string Key { get; set; }

    /// <summary>
    ///     new created document with the added values (only if the request has been executed with the parameter returNew=true
    /// </summary>
    public Dictionary<string, object> New { get; set; }

    /// <summary>
    ///     return the old document( only if the request has been executed with the parameter returOld=true)
    /// </summary>
    public Dictionary<string, object> Old { get; set; }

    /// <summary>
    ///     old revision of the document, if the current document overwrited someother
    /// </summary>
    [JsonProperty("_oldrev")]
    public string OldRev { get; set; }

    /// <summary>
    ///     contains the document revision
    /// </summary>
    [JsonProperty("_rev")]
    public string Rev { get; set; }
}
