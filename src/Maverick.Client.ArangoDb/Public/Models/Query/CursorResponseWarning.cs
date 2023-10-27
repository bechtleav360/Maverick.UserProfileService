namespace Maverick.Client.ArangoDb.Public.Models.Query;

/// <summary>
///     Contains cursor response warnings
/// </summary>
public class CursorResponseWarning
{
    /// <summary>
    ///     Error code
    /// </summary>
    public long Code { get; set; }

    /// <summary>
    ///     Error message
    /// </summary>
    public string Message { get; set; }
}
