using System.Collections.Generic;

namespace Maverick.Client.ArangoDb.Public.Models.Document;

/// <summary>
///     Contains basic attributes of a document with data contained in it
/// </summary>
/// <inheritdoc />
public class DocumentWithData : DocumentBase
{
    /// <summary>
    ///     Data contained in the specified document
    /// </summary>
    public Dictionary<string, object> DocumentData { get; set; }
}
