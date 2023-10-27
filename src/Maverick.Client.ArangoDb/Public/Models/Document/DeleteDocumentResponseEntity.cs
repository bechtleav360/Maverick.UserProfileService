using System.Collections.Generic;

namespace Maverick.Client.ArangoDb.Public.Models.Document;

/// <summary>
///     Conatins some attributes of a deleted document
/// </summary>
/// <inheritdoc />
public class DeleteDocumentResponseEntity : DocumentBase
{
    /// <summary>
    ///     deleted document (only if the parameter returnOld has been setted "true")
    /// </summary>
    public Dictionary<string, object> Old { get; set; }
}
