namespace UserProfileService.Queries.Language.ExpressionOperators;

/// <summary>
///     The combinator combines two expression with an binary operator.
///     The supported combinators are the "and" and "or"-combinators.
/// </summary>
public enum ExpressionCombinator
{
    /// <summary>
    ///     The none operator that is used when the binary operator is not known.
    ///     It is a fallback value.
    /// </summary>
    None,

    /// <summary>
    ///     The "and" -operator that is used to combine two expressions.
    /// </summary>
    And,

    /// <summary>
    ///     The or combinator that connects two expressions.
    /// </summary>
    Or
}
