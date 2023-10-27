using Maverick.UserProfileService.Models.Models;

namespace UserProfileService.Abstractions;

/// <summary>
///     Defines all methods that will change the data set that should be kept volatile. Volatile data won't be stored in an
///     event store (by event sourcing). It will be persisted directly and will be lost, if the regarding database is lost.
/// </summary>
public interface IVolatileDataOperationHandler
{
    /// <summary>
    ///     Starts a request to create a user settings section including <paramref name="userSettingObjects" />.
    /// </summary>
    /// <remarks>
    ///     It will create a request ticket that will contain the current status of the write operation.
    /// </remarks>
    /// <param name="userId">The id of the user profile whose settings should be modified.</param>
    /// <param name="userSettingsSectionName">The name of the settings section that should be created.</param>
    /// <param name="userSettingObjects">
    ///     The JSON array string of new setting keys/values that should be added to the new
    ///     section.
    /// </param>
    /// <param name="cancellationToken">A token to monitor cancellation requests.</param>
    /// <returns>
    ///     A task representing the asynchronous write operation. It wraps a request id referring to the status object of this
    ///     this operation.
    /// </returns>
    Task<string> CreateUserSettingAsync(
        string userId,
        string userSettingsSectionName,
        string userSettingObjects,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Starts a request to update the JSON body of a user settings object of a specified section.
    /// </summary>
    /// <remarks>
    ///     It will create a request ticket that will contain the current status of the write operation.
    /// </remarks>
    /// <param name="userId">The id of the user profile whose settings should be modified.</param>
    /// <param name="userSettingsSectionName">The name of the settings section whose child object should be modified.</param>
    /// <param name="userSettingsId">The id of the child of the user settings section that should be modified</param>
    /// <param name="userSettingsObject">
    ///     The JSON object (a document inside curly braces) as string that will replace the existing user settings object
    ///     value.
    /// </param>
    /// <param name="cancellationToken">A token to monitor cancellation requests.</param>
    /// <returns>
    ///     A task representing the asynchronous write operation. It wraps a request id referring to the status object of this
    ///     this operation.
    /// </returns>
    Task<string> UpdateUserSettingsAsync(
        string userId,
        string userSettingsSectionName,
        string userSettingsId,
        string userSettingsObject,
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
    ///     A task representing the asynchronous write operation. It wraps a request id referring to the status object of this
    ///     this operation.
    /// </returns>
    Task<string> DeleteSettingsSectionForUserAsync(
        string userId,
        string userSettingSectionName,
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
    ///     A task representing the asynchronous write operation. It wraps a request id referring to the status object of this
    ///     this operation.
    /// </returns>
    Task<string> DeleteUserSettingsAsync(
        string userId,
        string userSettingsSectionName,
        string userSettingsId,
        CancellationToken cancellationToken = default);
}
