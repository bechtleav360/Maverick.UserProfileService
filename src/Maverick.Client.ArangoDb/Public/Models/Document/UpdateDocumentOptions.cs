using System.Collections.Generic;

namespace Maverick.Client.ArangoDb.Public.Models.Document;

/// <summary>
///     Contains some parameters that can be setted by updating a document.
/// </summary>
public class UpdateDocumentOptions : DocumentOptions
{
    /// <summary>
    ///     By default, or if this is set to true, the _rev attributes in
    ///     the given documents are ignored. If this is set to false, then
    ///     any _rev attribute given in a body document is taken as a
    ///     precondition. The document is only updated if the current revision is the one specified
    /// </summary>
    public bool? IgnoreRevs { get; set; }

    /// <summary>
    ///     If the intention is to delete existing attributes with the patch
    ///     command, the URL query parameter keepNull can be used with a value
    ///     of false. This will modify the behavior of the patch command to
    ///     remove any attributes from the existing document that are contained
    ///     in the patch document with an attribute value of null
    /// </summary>
    public bool? KeepNull { get; set; }

    /// <summary>
    ///     Controls whether objects (not arrays) will be merged if present in
    ///     both the existing and the patch document. If set to false, the
    ///     value in the patch document will overwrite the existing document's
    ///     value. If set to true, objects will be merged. The default is true
    /// </summary>
    public bool? MergeObjects { get; set; }

    /// <summary>
    ///     Return additionally the complete new documents under the attribute "new" in the result
    /// </summary>
    public bool? ReturnNew { get; set; }

    internal string ToOptionsString()
    {
        var queryParams = new List<string>();

        if (KeepNull != null)
        {
            queryParams.Add("keepNull=" + KeepNull.ToString().ToLower());
        }

        if (MergeObjects != null)
        {
            queryParams.Add("mergeObjects=" + MergeObjects.ToString().ToLower());
        }

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

        return string.Join("&", queryParams);
    }
}
