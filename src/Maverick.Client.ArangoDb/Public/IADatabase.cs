using System.Collections.Generic;
using System.Threading.Tasks;
using Maverick.Client.ArangoDb.Public.Models.Collection;
using Maverick.Client.ArangoDb.Public.Models.Database;

// Implementation based on C# driver implementation from https://github.com/yojimbo87/ArangoDB-NET 
// with some bug fixes and extensions to support asynchronous operations 
namespace Maverick.Client.ArangoDb.Public;

/// <summary>
///     Provides access to ArangoDB Database endpoints.
/// </summary>
public interface IADatabase
{
    /// <summary>
    ///     Creates new database with the given name
    /// </summary>
    /// <param name="databaseName">Name of the created database</param>
    /// <param name="creationOptions">Additional options to configure newly created database.</param>
    /// <returns>
    ///     Object containing information about the created database or possibly occurred errors
    ///     <see cref="CreateDbResponse" />.
    /// </returns>
    Task<CreateDbResponse> CreateDatabaseAsync(
        string databaseName,
        DatabaseInfoEntityOptions creationOptions = null);

    /// <summary>
    ///     Creates new database with given name and user list.
    /// </summary>
    /// <param name="databaseName">Name of the created database</param>
    /// <param name="users">List of database users.</param>
    /// <param name="creationOptions">Additional options to configure newly created database.</param>
    /// <returns>
    ///     Object containing information about the created database or possibly occurred errors
    ///     <see cref="CreateDbResponse" />
    /// </returns>
    Task<CreateDbResponse> CreateDatabaseAsync(
        string databaseName,
        IList<AUser> users,
        DatabaseInfoEntityOptions creationOptions = null);

    /// <summary>
    ///     Retrieves information about currently connected database.
    /// </summary>
    /// <returns>
    ///     Object containing some information about the current database or possibly occurred errors
    ///     <see cref="GetCurrentDatabaseResponse" />.
    /// </returns>
    Task<GetCurrentDatabaseResponse> GetCurrentDatabaseInfoAsync();

    /// <summary>
    ///     Retrieves list of accessible databases which current user can access without specifying a different username or
    ///     password.
    /// </summary>
    /// <returns>
    ///     Object containing the list of the accessible databases or possibly occurred errors
    ///     <see cref="GetDatabasesResponse" />.
    /// </returns>
    Task<GetDatabasesResponse> GetAccessibleDatabasesAsync();

    /// <summary>
    ///     Retrieves the list of all existing databases.
    /// </summary>
    /// <returns>
    ///     Object containing the list of all existing databases or possibly occurred errors
    ///     <see cref="GetDatabasesResponse" />.
    /// </returns>
    Task<GetDatabasesResponse> GetAllDatabasesAsync();

    /// <summary>
    ///     Retrieves information about collections in current database connection.
    /// </summary>
    /// <returns>
    ///     Object containing the list of all Collections in the current database or possibly occurred errors
    ///     <see cref="GetAllCollectionsResponse" />.
    /// </returns>
    Task<GetAllCollectionsResponse> GetAllCollectionsAsync();

    /// <summary>
    ///     Retrieves information about collections in current database connection.
    /// </summary>
    /// <param name="excludeSystem">
    ///     If true, system collections should be filtered out.<br />
    ///     If this parameter is skipped, it will override the setting given by ExcludeSystem().
    /// </param>
    /// <returns>
    ///     Object containing the list of all Collections in the current database or possibly occurred errors
    ///     <see cref="GetAllCollectionsResponse" />.
    /// </returns>
    Task<GetAllCollectionsResponse> GetAllCollectionsAsync(bool excludeSystem);

    /// <summary>
    ///     Deletes specified database.
    /// </summary>
    /// <param name="databaseName">Database name</param>
    /// <returns>
    ///     Object containing some information about the deleted database or possibly occurred errors
    ///     <see cref="DropDbResponse" />
    /// </returns>
    Task<DropDbResponse> DropDatabaseAsync(string databaseName);
}
