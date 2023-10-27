namespace UserProfileService.Queries.Language.ExpressionOperators;

/// <summary>
///     The combinator operator that can connect expressions.
/// </summary>
public static class FilterExpressionCombinator
{
    /// <summary>
    ///     The and combinator to connect two expressions.
    /// </summary>
    public const string AndCombinator = "and";

    /// <summary>
    ///     The or combinator to connect two expression.
    /// </summary>
    public const string OrCombinator = "or";
}
