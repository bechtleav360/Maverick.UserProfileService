using System.Text.Json.Nodes;
using Maverick.UserProfileService.Models.Models;
using UserProfileService.Adapter.Marten.EntityModels;
using UserProfileService.Common.V2.Abstractions;
using UserProfileService.Common.V2.Models;

namespace UserProfileService.Adapter.Marten.Abstractions;

/// <summary>
///     The volatile user settings store contains volatile data
///     for a certain user. The store is used for storing volatile data
///     as changing and retrieve the data for a certain user.
/// </summary>
public interface IVolatileUserSettingsStore
{
    /// <summary>
    ///     Get all <see cref="UserSettingSection" /> that are stored in the volatile store
    ///     for a certain user.
    /// </summary>
    /// <param name="userId">The user id  which the <see cref="UserSettingSection" />s should be retrieved.</param>
    /// <param name="paginationQueryObject">Includes options to set pagination and sorting.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>
    ///     A task representing the asynchronous read operation. It wraps a  list of <see cref="UserSettingSection" /> that
    ///     represents the user settings section.
    /// </returns>
    Task<List<UserSettingSectionDbModel>> GetAllSettingSectionForUserAsync(
        string userId,
        QueryOptionsVolatileModel paginationQueryObject,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes a user settings section for an appropriate user in the volatile store.
    /// </summary>
    /// <param name="userId">The id of the user which user settings section should be deleted.</param>
    /// <param name="userSettingSectionName">The user settings section name that should be deleted.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>
    ///     A task representing the asynchronous write operation.
    /// </returns>
    Task DeleteSettingsSectionForUserAsync(
        string userId,
        string userSettingSectionName,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates new user settings objects for a certain user. Each JSON Object of the provided JSON array will be inserted
    ///     separately.
    /// </summary>
    /// <param name="userId">The user that should get the user settings object.</param>
    /// <param name="userSettingsSectionName">The user settings section name that is needed to store the value.</param>
    /// <param name="userSettingsValues">A collection of <see cref="JsonObject" /> as a <see cref="JsonArray" />.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>
    ///     A task representing the asynchronous write operation. It wraps <see cref="UserSettingObject" />
    ///     that represents the user settings object which was created.
    /// </returns>
    Task<IList<UserSettingObjectDbModel>> CreateUserSettingsAsync(
        string userId,
        string userSettingsSectionName,
        JsonArray userSettingsValues,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes a specific <see cref="UserSettingObject" /> in the volatile user store that is specified
    ///     though a user id, a user settings section name and a user settings id.
    /// </summary>
    /// <param name="userId">The user id which the user settings object will be deleted.</param>
    /// <param name="userSettingsSectionName">The user settings section name that is needed to delete the user settings value.</param>
    /// <param name="userSettingsId">
    ///     The user settings key that is used to delete the <see cref="UserSettingObject" /> in the
    ///     volatile store.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>
    ///     A task representing the asynchronous write operation.
    /// </returns>
    Task DeleteUserSettingsAsync(
        string userId,
        string userSettingsSectionName,
        string userSettingsId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Updates the user settings object specified though a user id, a user settings section name
    ///     and a user settings id.
    /// </summary>
    /// <param name="userId">The user id for which the user settings object should be updated.</param>
    /// <param name="userSettingsSectionName">
    ///     The user settings section name that is needed to update the user settings value.
    /// </param>
    /// <param name="userSettingsId">
    ///     The user settings key that is used to update the <see cref="UserSettingObject" /> in the
    ///     volatile store.
    /// </param>
    /// <param name="userSettingsValue">The user settings value that should be update.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>
    ///     A task representing the asynchronous write operation. It wraps an <see cref="UserSettingObject" />
    ///     that represents the user settings object which was updated.
    /// </returns>
    Task<UserSettingObjectDbModel> UpdateUserSettingsAsync(
        string userId,
        string userSettingsSectionName,
        string userSettingsId,
        JsonObject userSettingsValue,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Get a list of  <see cref="UserSettingObject" />s for a certain userId and a certain user section name.
    /// </summary>
    /// <param name="userId">The user id which the user settings objects should be retrieved.</param>
    /// <param name="userSettingsSectionName">
    ///     The user settings section name that is used to retrieve the <see cref="UserSettingObject" /> in the
    ///     volatile store.
    /// </param>
    /// <param name="paginationAndSorting">Includes options to set pagination and sorting.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>
    ///     A task representing the asynchronous write operation. It wraps <see cref="UserSettingObject" />
    ///     that represents the user settings objects which were retrieved.
    /// </returns>
    public Task<List<UserSettingObjectDbModel>> GetUserSettingsObjectsForUserAsync(
        string userId,
        string userSettingsSectionName,
        QueryOptionsVolatileModel paginationAndSorting,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Returns all existing user settings as a list of <see cref="UserSettingObject" />s. The list is related to the
    ///     provided user.
    /// </summary>
    /// <param name="userId">The user id that is needed to find all user settings objects.</param>
    /// <param name="paginationAndSorting">Includes options to set pagination and sorting.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>
    ///     A task representing the asynchronous read operation. The <see cref="IPaginatedList{TElem}" /> wraps
    ///     that represents all existing user user settings object which was retrieved for a certain user.
    /// </returns>
    Task<List<UserSettingObjectDbModel>> GetAllUserSettingsObjectForUserAsync(
        string userId,
        QueryOptionsVolatileModel paginationAndSorting,
        CancellationToken cancellationToken = default);
}
