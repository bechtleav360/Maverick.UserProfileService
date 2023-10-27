using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Maverick.Client.ArangoDb.Public.Models.Document;

/// <summary>
///     Contains some options that can be modified by creating a document
/// </summary>
/// <inheritdoc />
public class CreateDocumentOptions : DocumentOptions
{
    /// <summary>
    ///     If set to true, the insert becomes a replace-insert. If a document with the
    ///     same _key already exists the new document is not rejected with unique
    ///     constraint violated but will replace the old document
    /// </summary>
    public bool? Overwrite { get; set; }

    /// <summary>
    ///     This option supersedes overwrite and offers more detailed modes. It is a new feature in ArangoDB ver.3.7. and not
    ///     available in older versions.
    ///     <list type="bullet">
    ///         <item>
    ///             <term>ignore</term>
    ///             <description>
    ///                 if a document with the specified _key value exists already,
    ///                 nothing will be done and no write operation will be carried out. The
    ///                 insert operation will return success in this case. This mode does not
    ///                 support returning the old document version using RETURN OLD. When using
    ///                 RETURN NEW, null will be returned in case the document already existed.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <term>replace</term>
    ///             <description>
    ///                 if a document with the specified _key value exists already,
    ///                 it will be overwritten with the specified document value. This mode will
    ///                 also be used when no overwrite mode is specified but the overwrite
    ///                 flag is set to true.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <term>update</term>
    ///             <description>
    ///                 if a document with the specified _key value exists already,
    ///                 it will be patched (partially updated) with the specified document value.
    ///                 The overwrite mode can be further controlled via the keepNull and mergeObjects parameters.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <term>conflict</term>
    ///             <description>
    ///                 if a document with the specified _key value exists already,
    ///                 return a unique constraint violation error so that the insert operation
    ///                 fails. This is also the default behavior in case the overwrite mode is
    ///                 not set, and the overwrite flag is false or not set either.
    ///             </description>
    ///         </item>
    ///     </list>
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public AOverwriteMode? OverWriteMode { get; set; }

    /// <summary>
    ///     Additionally return the complete new document under the attribute new in the result.
    /// </summary>
    public bool? ReturnNew { get; set; }

    internal string ToOptionsString()
    {
        var query = new List<string>();

        if (WaitForSync != null)
        {
            query.Add("waitForSync=" + WaitForSync.ToString().ToLower());
        }

        if (ReturnNew != null)
        {
            query.Add("returnNew=" + ReturnNew.ToString().ToLower());
        }

        if (ReturnOld != null)
        {
            query.Add("returnOld=" + ReturnOld.ToString().ToLower());
        }

        if (Silent != null)
        {
            query.Add("silent=" + Silent.ToString().ToLower());
        }

        if (Overwrite != null)
        {
            query.Add("overwrite=" + Overwrite.ToString().ToLower());
        }

        if (OverWriteMode != null)
        {
            query.Add("overwriteMode=" + OverWriteMode.ToString().ToLower());
        }

        return string.Join("&", query);
    }
}
