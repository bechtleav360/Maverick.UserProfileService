namespace UserProfileService.Adapter.Marten.Options;

/// <summary>
///     Contains properties to set up a connection to PostgreSQL using the Marten library.
/// </summary>
public class MartenConnectionOptions
{
    /// <summary>
    ///     The connection string used to connect to PostgreSql.
    /// </summary>
    /// <remarks>
    ///     Must be parseable by <see href="https://www.npgsql.org/">Npgsql</see>.
    /// </remarks>
    public string? ConnectionString { get; set; }

    /// <summary>
    ///     The name of the database schema in the PostgreSql database.
    /// </summary>
    public string? DatabaseSchema { get; set; }
}
