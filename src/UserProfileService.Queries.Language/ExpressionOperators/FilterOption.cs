namespace UserProfileService.Queries.Language.ExpressionOperators;

/// <summary>
///     Filter options that are currently available.
/// </summary>
public enum FilterOption
{
    /// <summary>
    ///     If none operator has been chosen.
    /// </summary>
    None,

    /// <summary>
    ///     The $filter option that can filter though attributes.
    /// </summary>
    DollarFilter,

    /// <summary>
    ///     The $orderBy option can sort the result for a one or more attributes.
    /// </summary>
    DollarOrderBy
}
