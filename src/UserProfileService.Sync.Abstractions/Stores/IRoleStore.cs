using System.Threading;
using System.Threading.Tasks;
using Maverick.UserProfileService.Models.RequestModels;
using UserProfileService.Common.V2.Abstractions;
using UserProfileService.Sync.Abstraction.Models.Entities;
using UserProfileService.Validation.Abstractions;

namespace UserProfileService.Sync.Abstraction.Stores;

/// <summary>
///     Describes the store to handle roles.
/// </summary>
public interface IRoleStore
{
    /// <summary>
    ///     Gets roles.
    /// </summary>
    /// <remarks>
    ///     All found instances of roles will be converted to their estimated types<br />
    ///     (given by <typeparamref name="TRole" />)
    /// </remarks>
    /// <typeparam name="TRole">
    ///     The type of each element in the result set (either <see cref="RoleSync" /> or inherited from
    ///     <see cref="RoleSync" />).
    /// </typeparam>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <param name="options">
    ///     Options to refine the search request and to set up pagination and sorting. If <c>null</c>, the
    ///     default values of pagination will be used.
    /// </param>
    /// <returns>
    ///     A task representing the asynchronous read operation. It wraps a list of <see cref="RoleSync" />. If no roles
    ///     have been found, an empty list will be returned.
    /// </returns>
    /// <exception cref="ValidationException">If <paramref name="options" /> is not valid.</exception>
    Task<IPaginatedList<TRole>> GetRolesAsync<TRole>(
        QueryObject options = null,
        CancellationToken cancellationToken = default)
        where TRole : RoleSync;

    /// <summary>
    ///     Creates the provided role in the database
    /// </summary>
    /// <param name="role">   The <see cref="RoleSync" /> that is being created in the system</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>
    ///     A task representing the asynchronous read operation, containing the created operation of type
    ///     <see cref="RoleSync" />.
    /// </returns>
    Task<RoleSync> CreateRoleAsync(RoleSync role, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Updates the provided role in the database
    /// </summary>
    /// <param name="role">   The <see cref="RoleSync" /> that is being updated in the system</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>
    ///     A task representing the asynchronous read operation, containing the created operation of type
    ///     <see cref="RoleSync" />.
    /// </returns>
    Task<RoleSync> UpdateRoleAsync(RoleSync role, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes the role corresponding to the provided id in the database
    /// </summary>
    /// <param name="roleId">   The id of the <see cref="RoleSync" /> that is being deleted in the system</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />
    /// </param>
    Task DeleteRoleAsync(string roleId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Attempts to get the role by the given id. If not successful, <c>default(<see cref="RoleSync" />)</c> will be
    ///     returned.
    /// </summary>
    /// <param name="id">Id of role to retrieve.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>The role by given id.</returns>
    public Task<RoleSync> GetRoleAsync(string id, CancellationToken cancellationToken = default);
}
