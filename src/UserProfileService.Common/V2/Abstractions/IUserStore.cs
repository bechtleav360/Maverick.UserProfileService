using System.Threading;
using System.Threading.Tasks;

namespace UserProfileService.Common.V2.Abstractions;

/// <summary>
///     The user store retrieve a user from a data store. It also can check
///     if a user exists.
/// </summary>
public interface IUserStore
{
    /// <summary>
    ///     Returns true regarding the specified <paramref name="userId" />.
    ///     Otherwise the value false will be return if no user with the id exists.
    /// </summary>
    /// <param name="userId">
    ///     The userId identifies the user by its id.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>
    ///     A task representing the asynchronous read operation. It wraps a <see cref="bool" />.
    ///     If the user could be found a true is returned, otherwise false.
    /// </returns>
    Task<bool> CheckUserExistsAsync(
        string userId,
        CancellationToken cancellationToken = default);
}
