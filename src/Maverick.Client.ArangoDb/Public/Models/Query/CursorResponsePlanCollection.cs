namespace Maverick.Client.ArangoDb.Public.Models.Query;

/// <summary>
///     Contains plan collection informations
/// </summary>
public class CursorResponsePlanCollection
{
    /// <summary>
    ///     Name of plan collection
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     Type of plan collection
    /// </summary>
    public string Type { get; set; }
}
