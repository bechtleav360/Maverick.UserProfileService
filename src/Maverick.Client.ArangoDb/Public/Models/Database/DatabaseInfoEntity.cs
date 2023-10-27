namespace Maverick.Client.ArangoDb.Public.Models.Database;

/// <summary>
///     Contains informations about a database
/// </summary>
public class DatabaseInfoEntity : DatabaseInfoEntityOptions
{
    /// <summary>
    ///     the id of the database
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    ///     whether or not the database is the _system database
    /// </summary>
    public bool IsSystem { get; set; }

    /// <summary>
    ///     the name of the database
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     the filesystem path of the database
    /// </summary>
    public string Path { get; set; }
}
