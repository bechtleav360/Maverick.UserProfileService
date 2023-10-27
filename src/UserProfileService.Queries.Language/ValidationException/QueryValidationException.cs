namespace UserProfileService.Queries.Language.ValidationException;

/// <summary>
///     The <see cref="QueryValidationException" /> is used to inform that a filter query was not valid.
///     The message contains more information what exactly failed while validation the query.
/// </summary>
[Serializable]
public class QueryValidationException : Exception
{
    /// <summary>
    ///     An error message that can be used to categorize which filter query was affected.
    /// </summary>
    public string QuerySource { set; get; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="QueryValidationException" /> class, without setting the
    ///     <see cref="QuerySource" /> and the message properties.
    /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public QueryValidationException()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="QueryValidationException" /> class with a specified error message,
    ///     but without setting the <see cref="QuerySource" /> property.
    /// </summary>
    /// <param name="message">The error message string.</param>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public QueryValidationException(string message) : base(message)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="QueryValidationException" /> class with a specified error message and
    ///     a specified error code.
    /// </summary>
    /// <param name="querySource">An error message that can be used to categorize which filter query was affected.</param>
    /// <param name="message">The error message string.</param>
    public QueryValidationException(string querySource, string message) : base(message)
    {
        QuerySource = querySource;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="QueryValidationException" /> class with a specified error message, a
    ///     specified error code and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="querySource">An error message that can be used to categorize which filter query was affected.</param>
    /// <param name="message">The error message string.</param>
    /// <param name="inner">The inner exception reference.</param>
    public QueryValidationException(string querySource, string message, Exception inner)
        : base(message, inner)
    {
        QuerySource = querySource;
    }
}
