namespace UserProfileService.Adapter.Arango.V2.EntityModels.QueryBuilding;

/// <summary>
///     Marks an expression as AQL specified expression that can create AQL sub.queries.
/// </summary>
internal interface IAqlExpression
{
    /// <summary>
    ///     Gets the AQL string dependent on visited results.
    /// </summary>
    /// <returns></returns>
    public string GetAqlString();
}
