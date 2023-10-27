namespace UserProfileService.Queries.Language.ExpressionOperators;

/// <summary>
///     The expression operators contains the possible operators that can be used to
///     write an filter query.
/// </summary>
public static class ExpressionOperators
{
    /// <summary>
    ///     The  contains operator that is used in a query.
    /// </summary>
    public const string ContainsOperator = "ct";

    /// <summary>
    ///     The equal operator that is used in a query.
    /// </summary>
    public const string EqualsOperator = "eq";

    /// <summary>
    ///     The greater equals operator that is used in a query.
    /// </summary>
    public const string GreaterEqualsOperator = "ge";

    /// <summary>
    ///     The greater that operator that is used in a query.
    /// </summary>
    public const string GreaterThenOperator = "gt";

    /// <summary>
    ///     The less equals operator that is used in a query.
    /// </summary>
    public const string LessEqualsOperator = "le";

    /// <summary>
    ///     The less then operator that is used in a query.
    /// </summary>
    public const string LessThenOperator = "lt";

    /// <summary>
    ///     The not equal operator that is used in a query.
    /// </summary>
    public const string NotEqualsOperator = "ne";
}
