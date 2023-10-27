namespace UserProfileService.Queries.Language.ExpressionOperators;

/// <summary>
///     The operators that can be used by an expression.
///     The operator enum is used for an expression tree.
/// </summary>
public enum OperatorType

{
    /// <summary>
    ///     The operator was not found a fallback operator.
    /// </summary>
    None,

    /// <summary>
    ///     The equals operator that connects the left-hand-side with right-hand-side.
    /// </summary>
    EqualsOperator,

    /// <summary>
    ///     The not equals operator that connects the left-hand-side with right-hand-side.
    /// </summary>
    NotEqualsOperator,

    /// <summary>
    ///     The greater then operator that connects the left-hand-side with right-hand-side.
    /// </summary>
    GreaterThenOperator,

    /// <summary>
    ///     The greater equals operator that connects the left-hand-side with right-hand-side.
    /// </summary>
    GreaterEqualsOperator,

    /// <summary>
    ///     The less then operator that connects the left-hand-side with right-hand-side.
    /// </summary>
    LessThenOperator,

    /// <summary>
    ///     The less equals operator that connects the left-hand-side with right-hand-side.
    /// </summary>
    LessEqualsOperator,

    /// <summary>
    ///     The contains operator that connects the left-hand-side with the right-hand-side.
    /// </summary>
    Contains
}
