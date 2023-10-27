using System.Threading;
using System.Threading.Tasks;
using UserProfileService.Common.V2.Models;

namespace UserProfileService.Common.V2.Abstractions;

/// <summary>
///     Initializes a database before it is being used. This operation can contain checks, creation or modification of
///     stored schemes, table, collections, etc.
/// </summary>
public interface IDbInitializer
{
    /// <summary>
    ///     Ensures that the database for the context exists.
    /// </summary>
    /// <paramref name="forceRecreation">Forces a recreation of the database. This will destroy all data.</paramref>
    /// <paramref name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </paramref>
    /// >
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task<SchemaInitializationResponse> EnsureDatabaseAsync(
        bool forceRecreation = false,
        CancellationToken cancellationToken = default);
}
