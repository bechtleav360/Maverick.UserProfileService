namespace UserProfileService.Adapter.Arango.V2.EntityModels;

/// <summary>
///     Model class to determine an amount of a query.
/// </summary>
public class CountingModel
{
    /// <summary>
    ///     The current document count.
    /// </summary>
    public long DocumentCount { get; set; }
}
