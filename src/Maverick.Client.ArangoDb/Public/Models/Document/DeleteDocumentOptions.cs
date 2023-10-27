using System.Collections.Generic;

namespace Maverick.Client.ArangoDb.Public.Models.Document;

/// <summary>
///     Contains some options that can be setted by deleting of a document
/// </summary>
/// <inheritdoc />
public class DeleteDocumentOptions : DocumentOptions
{
    /// <summary>
    ///     you can conditionally remove a document based on a target revision id by using the if-match HTTP header
    /// </summary>
    public string IfMatch { get; set; }

    internal string ToOptionsString()
    {
        var query = new List<string>();

        if (WaitForSync != null)
        {
            query.Add("waitForSync=" + WaitForSync.ToString().ToLower());
        }

        if (ReturnOld != null)
        {
            query.Add("returnOld=" + ReturnOld.ToString().ToLower());
        }

        if (Silent != null)
        {
            query.Add("silent=" + Silent.ToString().ToLower());
        }

        return string.Join("&", query);
    }
}
