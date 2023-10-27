using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.BasicModels;
using Maverick.UserProfileService.Models.Models;
using UserProfileService.Common.V2.Contracts;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Models;

namespace UserProfileService.Common.V2.Abstractions;

/// <summary>
///     An interface defining all operations required by the projection in order to update the read-database.
/// </summary>
public interface IProjectionWriteService : IProjectionStateRepository
{
    /// <summary>
    ///     Specifies a name for the instance of <see cref="IProjectionWriteService" />.
    /// </summary>
    string Name { get; }

    /// <summary>
    ///     Creates the specified profile.
    /// </summary>
    /// <param name="profile">Contains the profile to create.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <param name="transaction">
    ///     The object includes information about the whole transaction. It also contains how the
    ///     transaction can be aborted.
    /// </param>
    /// <returns>A <see cref="Task" /> that represents the asynchronous write operation.</returns>
    /// <exception cref="ArgumentNullException">Will be thrown if the given profile is null.</exception>
    /// <exception cref="WebException">Will be thrown if an error occurred persisting the modified properties.</exception>
    Task CreateProfileAsync(
        IProfile profile,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes the specified profile.
    /// </summary>
    /// <param name="profileId">Specifies the id of the profile to delete.</param>
    /// <param name="transaction">
    ///     The object includes information about the whole transaction. It also contains how the
    ///     transaction can be aborted.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>A <see cref="Task" /> that represents the asynchronous write operation.</returns>
    /// <exception cref="ArgumentNullException">Will be thrown if the userId is null.</exception>
    /// <exception cref="ArgumentException">Will be thrown if the given userId is empty or only contains whitespaces.</exception>
    /// <exception cref="WebException">Will be thrown if an error occurred persisting the deletion.</exception>
    Task DeleteProfileAsync(
        string profileId,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Updates properties of the given profile.
    /// </summary>
    /// <param name="profileId">Specifies the id of the profile (user, group or oe) to update.</param>
    /// <param name="properties">Specifies the values to set.</param>
    /// <param name="transaction">
    ///     The object includes information about the whole transaction. It also contains how the
    ///     transaction can be aborted.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>A <see cref="Task" /> that represents the asynchronous write operation.</returns>
    /// <exception cref="ArgumentNullException">Will be thrown if either the profileId or the properties are set to null.</exception>
    /// <exception cref="ArgumentException">
    ///     Will be thrown if the given properties are empty or only contain keys which shall
    ///     not be modified.
    /// </exception>
    /// <exception cref="WebException">Will be thrown if an error occurred persisting the modified properties.</exception>
    Task UpdateProfileAsync(
        string profileId,
        IDictionary<string, object> properties,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Assigns profiles to roles, functions or container-profiles (groups and OrgUnits).
    /// </summary>
    /// <param name="assignments">The <see cref="Assignment" />s to create.</param>
    /// <param name="updatedAt">Specifies the time when the assignment was created.</param>
    /// <param name="transaction">
    ///     The object includes information about the whole transaction. It also contains how the
    ///     transaction can be aborted.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>A <see cref="Task" /> that represents the asynchronous write operation.</returns>
    /// <exception cref="ArgumentException">
    ///     If the type of target or any of the <see cref="ConditionObjectIdent" /> are of type
    ///     unknwon or no profile is included.
    /// </exception>
    /// <exception cref="ArgumentNullException">If target or elements is null.</exception>
    /// <exception cref="WebException">If an error occurred during the operation.</exception>
    Task AddProfileAssignmentsAsync(
        Assignment[] assignments,
        DateTime updatedAt,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Removes profile-assignments to roles, functions or container-profiles (groups and OrgUnits).
    /// </summary>
    /// <param name="assignments">The <see cref="Assignment" />s to create.</param>
    /// <param name="updatedAt">Specifies the time when the assignment was created.</param>
    /// <param name="transaction">
    ///     The object includes information about the whole transaction. It also contains how the
    ///     transaction can be aborted.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>A <see cref="Task" /> that represents the asynchronous write operation.</returns>
    /// <exception cref="ArgumentException">
    ///     If the type of target or any of the <see cref="ConditionObjectIdent" /> are of type
    ///     unknwon or no profile is included.
    /// </exception>
    /// <exception cref="ArgumentNullException">If target or elements is null.</exception>
    /// <exception cref="WebException">If an error occurred during the operation.</exception>
    Task RemoveProfileAssignmentsAsync(
        Assignment[] assignments,
        DateTime updatedAt,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates the specified function.
    /// </summary>
    /// <param name="function"></param>
    /// <param name="roleId">The id of the role which is associated to the role.</param>
    /// <param name="transaction">
    ///     The object includes information about the whole transaction. It also contains how the
    ///     transaction can be aborted.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>A <see cref="Task" /> that represents the asynchronous write operation.</returns>
    /// <exception cref="WebException">Will be thrown if an error occurred during the operation.</exception>
    /// <exception cref="ArgumentNullException">Will be thrown if the given function is to null.</exception>
    Task CreateFunctionAsync(
        FunctionBasic function,
        string roleId,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Updates properties within the specified function.
    /// </summary>
    /// <param name="functionId">The id of the function to update.</param>
    /// <param name="updatedProperties">The properties to update.</param>
    /// <param name="transaction">
    ///     The object includes information about the whole transaction. It also contains how the
    ///     transaction can be aborted.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>A <see cref="Task" /> that represents the asynchronous write operation.</returns>
    /// <exception cref="ArgumentNullException">Will be thrown if the profileId or the updatedProperties were null.</exception>
    /// <exception cref="ArgumentException">
    ///     Will be thrown if the functionId is empty, only contains whitespaces or the
    ///     updatedProperties contain no updateable properties.
    /// </exception>
    /// <exception cref="WebException">Will be thrown if an error occurred during the modification.</exception>
    Task UpdateFunctionAsync(
        string functionId,
        IDictionary<string, object> updatedProperties,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes the specified function.
    /// </summary>
    /// <param name="functionId">The id of the function to delete.</param>
    /// <param name="transaction">
    ///     The object includes information about the whole transaction. It also contains how the
    ///     transaction can be aborted.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>A <see cref="Task" /> that represents the asynchronous write operation</returns>
    /// <exception cref="ArgumentException">If the role specified in the function does not exist.</exception>
    /// <exception cref="ArgumentNullException">If the functionId specified null.</exception>
    /// <exception cref="ArgumentException">If the functionId specified is empty or whitespace.</exception>
    /// <exception cref="WebException">If an error occurred during the deletion ot the function.</exception>
    Task DeleteFunctionAsync(
        string functionId,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates the specified role.
    /// </summary>
    /// <param name="role"></param>
    /// <param name="transaction">
    ///     The object includes information about the whole transaction. It also contains how the
    ///     transaction can be aborted.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>A <see cref="Task" /> that represents the asynchronous write operation.</returns>
    /// <exception cref="ArgumentNullException">Will be thrown if the role is null.</exception>
    /// <exception cref="ArgumentException">
    ///     Will be thrown if the profileId is empty, only contains whitespaces or the tags
    ///     were empty.
    /// </exception>
    /// <exception cref="WebException">Will be thrown if an error occurred during the modification.</exception>
    Task CreateRoleAsync(
        RoleBasic role,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Updates properties within the specified role.
    /// </summary>
    /// <param name="roleId">The id of the role to update.</param>
    /// <param name="updatedProperties">The properties to update.</param>
    /// <param name="transaction">
    ///     The object includes information about the whole transaction. It also contains how the
    ///     transaction can be aborted.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>A <see cref="Task" /> that represents the asynchronous write operation.</returns>
    /// <exception cref="ArgumentNullException">Will be thrown if either the roleId or the updatedProperties are set to null.</exception>
    /// <exception cref="ArgumentException">
    ///     Will be thrown if the roleId is empty, only contains whitespaces or the
    ///     updatedProperties contain no editable property.
    /// </exception>
    /// <exception cref="WebException">Will be thrown if an error occurred during the modification.</exception>
    Task UpdateRoleAsync(
        string roleId,
        IDictionary<string, object> updatedProperties,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes the specified role.
    /// </summary>
    /// <param name="roleId">The id of the role to delete.</param>
    /// <param name="transaction">
    ///     The object includes information about the whole transaction. It also contains how the
    ///     transaction can be aborted.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>A <see cref="Task" /> that represents the asynchronous write operation</returns>
    /// <exception cref="ArgumentNullException">If the role specified null.</exception>
    /// <exception cref="ArgumentException">If the role specified is empty or whitespace.</exception>
    /// <exception cref="WebException">If an error occurred during the deletion ot the role.</exception>
    Task DeleteRoleAsync(
        string roleId,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Recalculates the given user-profiles based on the command-data.
    /// </summary>
    /// <param name="profiles">Specifies all profile-ids to recalculate. If set to null, all profiles will be recalculated.</param>
    /// <param name="recalculateParents">Specifies whether the parent-groups shall be recalculated as well.</param>
    /// <param name="transaction">
    ///     The object includes information about the whole transaction. It also contains how the
    ///     transaction can be aborted.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>A <see cref="Task" /> wrapping the write-operation.</returns>
    Task RecalculateProfileAsync(
        string[] profiles = null,
        bool recalculateParents = false,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Recalculates the given roles based on the command-data.
    /// </summary>
    /// <param name="profiles">
    ///     Specifies all role-ids and function-ids to recalculate. If set to null, all roles and functions
    ///     will be recalculated.
    /// </param>
    /// <param name="transaction">
    ///     The object includes information about the whole transaction. It also contains how the
    ///     transaction can be aborted.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>A <see cref="Task" /> wrapping the write-operation.</returns>
    Task RecalculateRolesOrFunctionsAsync(
        string[] profiles = null,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Starts a transaction and returns an object containing information about the new created transaction.
    /// </summary>
    /// <param name="context">The related context about the calling service.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />
    /// </param>
    /// <param name="revision">
    ///     Specifies a string-value which will be injected at write-time into all entries.
    ///     The value does not have any impact to functionality and is purely meant for troubleshooting purposes.
    /// </param>
    /// <returns>A task representing the asynchronous operation that wraps the <see cref="IDatabaseTransaction" />.</returns>
    Task<IDatabaseTransaction> StartTransactionAsync(
        string revision = null,
        CallingServiceContext context = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Aborts an existing <paramref name="transaction" />.
    /// </summary>
    /// <param name="transaction">The object including information about the transaction to aborted.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>A task representing the asynchronous write operation.</returns>
    Task AbortTransactionAsync(
        IDatabaseTransaction transaction,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Commits an existing <paramref name="transaction" />.
    /// </summary>
    /// <param name="transaction">The object including information about the transaction to commit.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>A task representing the asynchronous write operation.</returns>
    Task CommitTransactionAsync(
        IDatabaseTransaction transaction,
        CancellationToken cancellationToken = default);
}
