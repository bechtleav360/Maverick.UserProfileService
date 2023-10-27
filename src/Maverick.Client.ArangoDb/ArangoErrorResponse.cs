using System.Net;
using Maverick.Client.ArangoDb.Public;

namespace Maverick.Client.ArangoDb;

/// <summary>
///     Contains some information about an ArangoDB Error.
/// </summary>
public class ArangoErrorResponse
{
    /// <summary>
    ///     HTTP status code.
    /// </summary>
    public HttpStatusCode Code { get; set; }

    /// <summary>
    ///     Whether this is an error response (always true).
    /// </summary>
    public bool Error { get; set; }

    /// <summary>
    ///     Error message.
    /// </summary>
    public string ErrorMessage { get; set; }

    /// <summary>
    ///     ArangoDB error number.
    ///     See https://www.arangodb.com/docs/stable/appendix-error-codes.html for error numbers and descriptions.
    /// </summary>
    public AStatusCode ErrorNum { get; set; } = AStatusCode.ErrorNoError;

    /// <inheritdoc />
    public override string ToString()
    {
        return Error ? $"Error: Code: {ErrorNum}; Message: {ErrorMessage}" : null;
    }
}
