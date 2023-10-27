namespace Maverick.Client.ArangoDb.Public.Configuration;

/// <summary>
///     Configuration to connect arango client.
/// </summary>
public class ArangoClientConfiguration
{
    /// <summary>
    ///     The name of arango client instance to use.
    /// </summary>
    public string ClientName { get; set; } = AConstants.ArangoClientName;

    /// <summary>
    ///     Connection string to use to connect to arango.
    /// </summary>
    public string ConnectionString { get; set; }

    /// <summary>
    ///     Options, how to handle with arango exceptions.
    /// </summary>
    public ArangoExceptionOptions ExceptionOptions { get; set; } = new ArangoExceptionOptions();
}
