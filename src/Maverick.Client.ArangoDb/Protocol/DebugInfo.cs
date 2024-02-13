namespace Maverick.Client.ArangoDb.Protocol;

/// <summary>
///     Contains information to debug.
/// </summary>
public class DebugInfo
{
    /// <summary>
    ///     Request Execution time in milliseconds.
    /// </summary>
    public long ExecutionTime { get; set; }

    /// <summary>
    ///     Contains information about the used http method.
    /// </summary>
    public string RequestHttpMethod { get; set; }

    /// <summary>
    ///     Contains the body of the request
    /// </summary>
    public string RequestJsonBody { get; set; }

    /// <summary>
    ///     Contains the uri string of the request.
    /// </summary>
    public string RequestUri { get; set; }

    /// <summary>
    ///     The transaction id that is related to the request.
    /// </summary>
    public string TransactionId { get; set; }

    /// <summary>
    ///     Create an instance of DebugInfo
    /// </summary>
    public DebugInfo()
    {
    }

    /// <summary>
    ///     Create an new instance od DebugInfo using an old existing instance
    /// </summary>
    /// <param name="old"></param>
    public DebugInfo(DebugInfo old)
    {
        RequestUri = old?.RequestUri;
        RequestJsonBody = old?.RequestJsonBody;
        RequestHttpMethod = old?.RequestHttpMethod;
    }

    /// <summary>
    ///     Print the Request method with the request Uri
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"{RequestHttpMethod?.ToUpperInvariant() ?? "UNKNOWN"} {RequestUri}";
    }
}
