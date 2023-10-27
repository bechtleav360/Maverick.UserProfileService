using System.Collections.Generic;

namespace Maverick.Client.ArangoDb.Public.Models.Document;

/// <summary>
///     Contains some options that can be setted by replacing a document
/// </summary>
/// <inheritdoc />
public class ReplaceDocumentOptions : DocumentOptions
{
    /// <summary>
    ///     By default, or if this is set to true, the _rev attributes in
    ///     the given document is ignored.If this is set to false, then
    ///     the _rev attribute given in the body document is taken as a
    ///     precondition. The document is only replaced if the current revision
    ///     is the one specified.
    /// </summary>
    public bool? IgnoreRevs { get; set; }

    /// <summary>
    ///     Return additionally the complete new document under the attribute new
    ///     in the result
    /// </summary>
    public bool? ReturnNew { get; set; }

    internal string ToOptionsString()
    {
        var queryParams = new List<string>();

        if (WaitForSync != null)
        {
            queryParams.Add("waitForSync=" + WaitForSync.ToString().ToLower());
        }

        if (IgnoreRevs != null)
        {
            queryParams.Add("ignoreRevs=" + IgnoreRevs.ToString().ToLower());
        }

        if (ReturnOld != null)
        {
            queryParams.Add("returnOld=" + ReturnOld.ToString().ToLower());
        }

        if (ReturnNew != null)
        {
            queryParams.Add("returnNew=" + ReturnNew.ToString().ToLower());
        }

        if (Silent != null)
        {
            queryParams.Add("silent=" + ReturnNew.ToString().ToLower());
        }

        return string.Join("&", queryParams);
    }
}
