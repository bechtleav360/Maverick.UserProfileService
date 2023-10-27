namespace UserProfileService.Adapter.Arango.V2.Abstractions;

/// <summary>
///     Contains methods to convert a source type to registered target type suitable for an AQL query.
/// </summary>
internal interface IEntityPropertyAqlMapper
{
    /// <summary>
    ///     Returns a json string suitable to map input variable to a valid json document (can be used i.e.in RETURN
    ///     statement).
    /// </summary>
    /// <param name="inputVariable">The iterator name as a reference.</param>
    /// <returns>A json document as string (=> { "property":value,...}).</returns>
    string GetConvertingAqlQuery(string inputVariable);
}
