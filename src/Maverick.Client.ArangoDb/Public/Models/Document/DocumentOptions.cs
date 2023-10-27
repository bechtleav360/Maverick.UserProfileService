namespace Maverick.Client.ArangoDb.Public.Models.Document;

/// <summary>
///     Contains options that can be used for some document operations (create, delete, update, etc.)
/// </summary>
public class DocumentOptions
{
    /// <summary>
    ///     Additionally return the complete old document under the attribute old
    ///     in the result.Only available if the overwrite option is used.
    /// </summary>
    public bool? ReturnOld { get; set; }

    /// <summary>
    ///     If set to true, an empty object will be returned as response. No meta-data
    ///     will be returned for the created document.This option can be used to
    ///     save some network traffic
    /// </summary>
    public bool? Silent { get; set; }

    /// <summary>
    ///     Wait until document has been synced to disk
    /// </summary>
    public bool? WaitForSync { get; set; }
}
