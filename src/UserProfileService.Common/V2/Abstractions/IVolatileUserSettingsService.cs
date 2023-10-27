using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Maverick.UserProfileService.Models.Models;
using Maverick.UserProfileService.Models.RequestModels;

namespace UserProfileService.Common.V2.Abstractions;

/// <summary>
///     The volatile user store service takes care of storing the user settings
///     for a certain user. It checks if the users exists and handles all necessary
///     operation like crate, update, delete and retrieve user settings objects.
///     The volatile data are always related to a certain user.
/// </summary>
public interface IVolatileUserSettingsService
{
    /// <summary>
    ///     Returns the meta data of all stored user settings sections related to the requested user.
    /// </summary>
    /// <param name="userId">The id of the user which settings sections should be returned.</param>
    /// <param name="paginationAndSorting">Includes options for filtering like set pagination and sorting.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>
    ///     A task representing the asynchronous read operation. The <see cref="IPaginatedList{TElem}" /> wraps
    ///     <see cref="UserSettingSection" />s that represents the user settings sections related to the appropriate
    ///     user.
    /// </returns>
    Task<IPaginatedList<UserSettingSection>> GetAllSettingsSectionsAsync(
        string userId,
        QueryOptions paginationAndSorting = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Returns all user settings as paginated list of <see cref="JsonObject" />s. The list is related to the provided
    ///     user and user settings section name.
    /// </summary>
    /// <param name="userId">The user id that is needed to find a user settings objects.</param>
    /// <param name="userSettingsSectionName">The user settings key to get the user settings objects.</param>
    /// <param name="paginationAndSorting">Includes options to set pagination and sorting.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>
    ///     A task representing the asynchronous read operation. The <see cref="IPaginatedList{TElem}" /> wraps
    ///     that represents the user settings object which was retrieved for a certain user and user settings section name.
    /// </returns>
    Task<IPaginatedList<UserSettingObject>> GetUserSettingsAsync(
        string userId,
        string userSettingsSectionName,
        QueryOptions paginationAndSorting = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Returns all existing user settings as paginated list of <see cref="JsonObject" />s. The list is related to the
    ///     provided user.
    /// </summary>
    /// <param name="userId">The user id that is needed to find user settings objects.</param>
    /// <param name="paginationAndSorting">Includes options to set pagination and sorting.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>
    ///     A task representing the asynchronous read operation. The <see cref="IPaginatedList{TElem}" /> wraps
    ///     that represents all existing user user settings objects which were retrieved for a certain user.
    /// </returns>
    Task<IPaginatedList<UserSettingObject>> GetAllUserSettingObjectForUserAsync(
        string userId,
        QueryOptions paginationAndSorting = null,
        CancellationToken cancellationToken = default);
}
