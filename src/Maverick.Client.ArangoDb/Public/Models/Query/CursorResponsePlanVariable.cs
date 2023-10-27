namespace Maverick.Client.ArangoDb.Public.Models.Query;

/// <summary>
///     Contains informatiosn about plan variable.
/// </summary>
public class CursorResponsePlanVariable
{
    /// <summary>
    ///     Id of the  plan variable.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    ///     Name of the plan variable
    /// </summary>
    public string Name { get; set; }
}
