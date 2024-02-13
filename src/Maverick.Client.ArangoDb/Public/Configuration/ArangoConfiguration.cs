using UserProfileService.Common.V2.Abstractions;

namespace Maverick.Client.ArangoDb.Public.Configuration;

/// <summary>
///     The configuration fot the ArangoDB provider.
/// </summary>
public class ArangoConfiguration
{
    /// <summary>
    ///     The configuration regarding cluster installations of ArangoDB.
    /// </summary>
    public ArangoClusterConfiguration ClusterConfiguration { get; set; }

    /// <summary>
    ///     The connection string to connect to ArangoDB service.
    /// </summary>
    public string ConnectionString { get; set; }

    /// <summary>
    ///     The configuration regarding exception handling of ArangoDB.
    /// </summary>
    public ArangoExceptionConfiguration ExceptionConfiguration { get; set; } = new ArangoExceptionConfiguration();

    /// <summary>
    ///     The minutes until another check on the database is done by the <see cref="IDbInitializer" />.
    /// </summary>
    public int MinutesBetweenChecks { get; set; } = 480;

    /// <inheritdoc />
    public override string ToString()
    {
        return ConnectionString;
    }
}
