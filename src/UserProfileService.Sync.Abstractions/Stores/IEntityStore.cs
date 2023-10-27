using System.Threading;
using System.Threading.Tasks;
using Maverick.UserProfileService.Models.RequestModels;
using UserProfileService.Common.V2.Abstractions;
using UserProfileService.Sync.Abstraction.Models;
using UserProfileService.Sync.Abstraction.Models.Entities;

namespace UserProfileService.Sync.Abstraction.Stores;

/// <summary>
///     Describes the store to handle groups, users and projection state.
/// </summary>
public interface IEntityStore : IRoleStore, IFunctionStore
{
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
    ///     Get profile with the given id.
    /// </summary>
    /// <typeparam name="TProfile"> Profile type (UserSync,GroupSync or OrganizationSync).</typeparam>
    /// <param name="profileId">    The profile id.</param>
    /// <param name="cancellationToken">    Propagates notification that operations should be canceled.</param>
    /// <returns>   The profile corresponding to given id.</returns>
    public Task<TProfile> GetProfileAsync<TProfile>(
        string profileId,
        CancellationToken cancellationToken = default) where TProfile : ISyncProfile;

    /// <summary>
    ///     Update profile with the given properties
    /// </summary>
    /// <typeparam name="TProfile"> The profile type</typeparam>
    /// <param name="profile">  Profile that should be updated</param>
    /// <param name="cancellationToken">    Propagates notification that operations should be canceled.</param>
    /// <returns>   The updated profile</returns>
    public Task<TProfile> UpdateProfileAsync<TProfile>(
        TProfile profile,
        CancellationToken cancellationToken = default) where TProfile : ISyncProfile;

    /// <summary>
    ///     Creates a new profile in the database
    /// </summary>
    /// <typeparam name="TProfile"> The profile type</typeparam>
    /// <param name="profile">  The profile should be created</param>
    /// <param name="cancellationToken">    Propagates notification that operations should be canceled.</param>
    /// <returns>   Task containing the created profile</returns>
    Task<TProfile> CreateProfileAsync<TProfile>(TProfile profile, CancellationToken cancellationToken = default)
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
