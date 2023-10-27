using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Maverick.UserProfileService.Models.Models;
using Maverick.UserProfileService.Models.RequestModels;
using Microsoft.Extensions.Logging;
using UserProfileService.Common.V2.Abstractions;
using UserProfileService.Common.V2.Exceptions;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Models;
using UserProfileService.Sync.Abstraction.Models;
using UserProfileService.Sync.Abstraction.Models.Entities;

namespace UserProfileService.Sync.Projection.Abstractions;

/// <summary>
///     Defines the service to handle user and groups operations.
/// </summary>
public interface IProfileService : IRoleService, IFunctionService
{
    /// <summary>
    ///     Tries to save projection state and returns a boolean value indicating the success of the operation. An optional
    ///     <see cref="ILogger" /> can be used to log error or warnings.
    /// </summary>
    /// <param name="projectionState">The state to be saved.</param>
    /// <param name="transaction">
    ///     An optional parameter containing information about the current transaction. This won't be
    ///     used for a second try of the write operation.
    /// </param>
    /// <param name="logger">The logger to be used.</param>
    /// <param name="cancellationToken">The token to monitor cancellation requests.</param>
    /// <returns>
    ///     A task representing the asynchronous write operation. It wraps a boolean value that will be <c>true</c>, if
    ///     the operation has been successful, otherwise <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    public Task<bool> TrySaveProjectionStateAsync(
        ProjectionState projectionState,
        IDatabaseTransaction transaction = null,
        ILogger logger = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets the numbers of the latest events per stream previously projected by a handler.
    /// </summary>
    /// <param name="stoppingToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>
    ///     A task representing the asynchronous read operation. It wraps a dictionary containing all latest events per
    ///     streams.
    /// </returns>
    Task<Dictionary<string, ulong>> GetLatestProjectedEventIdsAsync(
        CancellationToken stoppingToken = default);

    /// <summary>
    ///     Get the list of all users.
    /// </summary>
    /// <param name="options">  The query options <see cref="AssignmentQueryObject" /></param>
    /// <param name="cancellationToken">    Propagates notification that operations should be canceled.</param>
    /// <returns>    <see cref="IPaginatedList{UserSync}" /> containing all users</returns>
    public Task<IPaginatedList<UserSync>> GetUsersAsync(
        AssignmentQueryObject options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Get the list of all groups.
    /// </summary>
    /// <param name="options">  The query options <see cref="AssignmentQueryObject" /></param>
    /// <param name="cancellationToken">    Propagates notification that operations should be canceled.</param>
    /// <returns>    <see cref="IPaginatedList{UserSync}" /> containing all groups</returns>
    public Task<IPaginatedList<GroupSync>> GetGroupsAsync(
        AssignmentQueryObject options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Get the list of all organizations.
    /// </summary>
    /// <param name="options">  The query options <see cref="AssignmentQueryObject" /></param>
    /// <param name="cancellationToken">    Propagates notification that operations should be canceled.</param>
    /// <returns>    <see cref="IPaginatedList{UserSync}" /> containing all organizations</returns>
    public Task<IPaginatedList<OrganizationSync>> GetOrganizationsAsync(
        AssignmentQueryObject options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets a profile.
    /// </summary>
    /// <param name="profileId">An unique id of the profile.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>A task representing the asynchronous read operation. It wraps the requested <typeparamref name="TProfile" />.</returns>
    /// <exception cref="ArgumentException"><paramref name="profileId" /> is empty or contains only whitespace characters.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="profileId" /> is <c>null</c>.</exception>
    /// <exception cref="InstanceNotFoundException">No profile can be found considering <paramref name="profileId" />.</exception>
    Task<TProfile> GetProfileAsync<TProfile>(
        string profileId,
        CancellationToken cancellationToken = default)
        where TProfile : ISyncProfile;

    /// <summary>
    ///     Returns the information about the global position of the latest projected event.
    /// </summary>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>A task representing the asynchronous read operation. It wraps the information about the global position.</returns>
    Task<GlobalPosition> GetPositionOfLatestProjectedEventAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates the provided profile in the database
    /// </summary>
    /// <typeparam name="TProfile">
    ///     The type of the profile that is being created, should from type <see cref="ISyncProfile" />
    /// </typeparam>
    /// <param name="profile">   The object that is being created in the system</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>
    ///     A task representing the asynchronous read operation, containing the created operation of type
    ///     <typeparamref name="TProfile"/>.
    /// </returns>
    Task<TProfile> CreateProfileAsync<TProfile>(TProfile profile, CancellationToken cancellationToken = default)
        where TProfile : ISyncProfile;

    /// <summary>
    ///     Updates the profile in the database
    /// </summary>
    /// <typeparam name="TProfile">
    ///     The type of the profile that is being updated, should from type <see cref="ISyncProfile" />
    /// </typeparam>
    /// <param name="profile">  The object that is being updated in the system</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>
    ///     A task representing the asynchronous read operation, containing the created operation of type
    ///     <typeparamref name="TProfile"/>.
    /// </returns>
    Task<TProfile> UpdateProfileAsync<TProfile>(TProfile profile, CancellationToken cancellationToken = default)
        where TProfile : ISyncProfile;

    /// <summary>
    ///     Deletes profile corresponding to the given id
    /// </summary>
    /// <typeparam name="TProfile"> Profile type</typeparam>
    /// <param name="id">   The id of the profile that should be deleted</param>
    /// <param name="cancellationToken">    Propagates notification that operations should be canceled.</param>
    /// <returns></returns>
    Task DeleteProfileAsync<TProfile>(string id, CancellationToken cancellationToken = default)
        where TProfile : ISyncProfile;
}
