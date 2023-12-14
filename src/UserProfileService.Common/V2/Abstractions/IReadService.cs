using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.BasicModels;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using Maverick.UserProfileService.Models.RequestModels;
using Newtonsoft.Json.Linq;
using UserProfileService.Common.V2.Exceptions;

namespace UserProfileService.Common.V2.Abstractions;

/// <summary>
///     The interface defines read operations related to profiles.
///     Mostly the profile related methods show which profile is assigned
///     to which object or profile.
/// </summary>
public interface IReadService
{
    /// <summary>
    ///     Gets all profiles using specified <paramref name="options" />.
    /// </summary>
    /// <remarks>
    ///     All found instances of users or groups will be converted to their estimated types<br />
    ///     (given by <typeparamref name="TGroup" /> and <typeparamref name="TUser" />)
    /// </remarks>
    /// <param name="expectedKind">The profile kind of each profile to be retrieved (optional parameter).</param>
    /// <param name="options">
    ///     Options to refine the list request and to set up pagination and sorting. If <c>null</c>, the
    ///     default values of pagination will be used.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <typeparam name="TUser">The type of the user instances to be returned.</typeparam>
    /// <typeparam name="TGroup">The type of the group instances to be returned.</typeparam>
    /// ///
    /// <typeparam name="TOrgUnit">The type of the organizational unit instances to be returned.</typeparam>
    /// <returns>
    ///     A task representing the asynchronous read operation. It wraps a list of found <see cref="IProfile" />s. It can
    ///     contain both users and groups.
    /// </returns>
    /// <exception cref="ValidationException"><paramref name="options" /> is not valid.</exception>
    Task<IPaginatedList<IProfile>> GetProfilesAsync<TUser, TGroup, TOrgUnit>(
        RequestedProfileKind expectedKind = RequestedProfileKind.All,
        AssignmentQueryObject options = null,
        CancellationToken cancellationToken = default)
        where TUser : UserBasic
        where TGroup : GroupBasic
        where TOrgUnit : OrganizationBasic;

    /// <summary>
    ///     Gets all profiles containing the provided tag, using specified <paramref name="options" />.
    /// </summary>
    /// <remarks>
    ///     All found instances of users or groups will be converted to their estimated types<br />
    ///     (given by <typeparamref name="TGroup" /> and <typeparamref name="TUser" />)
    /// </remarks>
    /// <param name="tag"> The provided tag, that should be contained in all returned profiles. </param>
    /// <param name="expectedKind"> The profile kind of each profile to be retrieved (optional parameter). </param>
    /// <param name="options">
    ///     Options to refine the list request and to set up pagination and sorting. If <c> null </c>, the
    ///     default values of pagination will be used.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <typeparam name="TUser"> The type of the user instances to be returned. </typeparam>
    /// <typeparam name="TGroup"> The type of the group instances to be returned. </typeparam>
    /// ///
    /// <typeparam name="TOrgUnit"> The type of the organizational unit instances to be returned. </typeparam>
    /// <returns>
    ///     A task representing the asynchronous read operation. It wraps a list of found <see cref="IProfile" />s. It can
    ///     contain both users and groups.
    /// </returns>
    /// <exception cref="ValidationException"> <paramref name="options" /> is not valid. </exception>
    Task<IPaginatedList<IProfile>> GetProfilesWithTagAsync<TUser, TGroup, TOrgUnit>(
        string tag,
        RequestedProfileKind expectedKind = RequestedProfileKind.All,
        QueryObject options = null,
        CancellationToken cancellationToken = default)
        where TUser : UserBasic
        where TGroup : GroupBasic
        where TOrgUnit : OrganizationBasic;

    /// <summary>
    ///     Gets the profiles for the given collection of ids..
    /// </summary>
    /// <remarks>
    ///     All found instances of users or groups will be converted to their estimated types<br />
    ///     (given by <typeparamref name="TGroup" /> and <typeparamref name="TUser" />)
    /// </remarks>
    /// <param name="profileIds">The collection of identifiers to retrieve the profiles for.</param>
    /// <param name="expectedKind">The profile kind of each profile to be retrieved (optional parameter).</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <typeparam name="TUser">The type of the user instances to be returned.</typeparam>
    /// <typeparam name="TGroup">The type of the group instances to be returned.</typeparam>
    /// ///
    /// <typeparam name="TOrgUnit">The type of the organizational unit instances to be returned.</typeparam>
    /// <returns>
    ///     A task representing the asynchronous read operation. It wraps a list of found <see cref="IProfile" />s. It can
    ///     contain both users and groups.
    /// </returns>
    Task<IPaginatedList<IProfile>> GetProfilesAsync<TUser, TGroup, TOrgUnit>(
        IEnumerable<string> profileIds,
        RequestedProfileKind expectedKind = RequestedProfileKind.All,
        CancellationToken cancellationToken = default)
        where TUser : UserBasic
        where TGroup : GroupBasic
        where TOrgUnit : OrganizationBasic;

    /// <summary>
    ///     Gets a profile.
    /// </summary>
    /// <param name="profileId">An unique id of the profile.</param>
    /// <param name="expectedKind">The profile kind of each profile to be retrieved.</param>
    /// <param name="includeInactiveAssignments">
    ///     Specifies whether properties regarding members or parents should contain
    ///     inactive assignments or not.
    /// </param>
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
        RequestedProfileKind expectedKind,
        bool includeInactiveAssignments = true,
        CancellationToken cancellationToken = default)
        where TProfile : IProfile;

    /// <summary>
    ///     Gets a profile by its id or external id. The result will be <c>null</c>, if no profile could be found.
    /// </summary>
    /// <param name="idOrExternalId">
    ///     The id used to get the profile - either by comparing its value to the unique profile id or
    ///     to one of the external ids.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>
    ///     A task representing the asynchronous read operation. It wraps the requested <typeparamref name="TUser" />,
    ///     <typeparamref name="TGroup" /> or <typeparamref name="TOrgUnit" />.
    /// </returns>
    /// <exception cref="ArgumentException"><paramref name="idOrExternalId" /> is empty or contains only whitespace characters.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="idOrExternalId" /> is <c>null</c>.</exception>
    Task<IProfile> GetProfileByIdOrExternalIdAsync<TUser, TGroup, TOrgUnit>(
        string idOrExternalId,
        CancellationToken cancellationToken = default)
        where TUser : IProfile
        where TGroup : IContainerProfile
        where TOrgUnit : IContainerProfile;

    /// <summary>
    ///     Search for profiles.
    /// </summary>
    /// <param name="options">
    ///     Options to refine the search request and to set up pagination and sorting. If <c>null</c>, the
    ///     default values of pagination will be used.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <typeparam name="T">
    ///     The type of objects in the resulting list. If type is an interface, the basic type of its
    ///     implementation will be returned (if available).
    /// </typeparam>
    /// <returns>
    ///     A task representing the asynchronous read operation. It wraps a search result list of
    ///     <typeparamref name="T" />.
    /// </returns>
    /// <exception cref="ValidationException"><paramref name="options" /> is not valid.</exception>
    Task<IPaginatedList<T>> SearchAsync<T>(
        QueryObject options = null,
        CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    ///     Returns the root container profiles that do not have any parents (or: that are not assigned to another container
    ///     profile).
    /// </summary>
    /// <param name="options">Refines the list request.</param>
    /// <param name="expectedKind">The profile kind of each profile to be retrieved.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>
    ///     A task representing the asynchronous read operation. It wraps a list of <see cref="IContainerProfile" />s that
    ///     are the roots of the specified profile.
    /// </returns>
    /// <typeparam name="TGroup">The type of each group to be returned.</typeparam>
    /// <typeparam name="TOrgUnit">The type of each organizational unit to be returned.</typeparam>
    /// <exception cref="ValidationException"><paramref name="options" /> is not valid.</exception>
    Task<IPaginatedList<IContainerProfile>> GetRootProfilesAsync<TGroup, TOrgUnit>(
        RequestedProfileKind expectedKind,
        AssignmentQueryObject options = null,
        CancellationToken cancellationToken = default)
        where TGroup : GroupBasic
        where TOrgUnit : OrganizationBasic;

    /// <summary>
    ///     Returns the direct parent profiles of the requested container profile.
    ///     The resulting items will be of type <see cref="IContainerProfile" /> and of requested profile kind.
    /// </summary>
    /// <param name="profileId">The id of the user or group whose parents are to be returned.</param>
    /// <param name="expectedKind">The profile kind of each profile to be retrieved.</param>
    /// <param name="options">Refines the list request.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>
    ///     A task representing the asynchronous read operation. It wraps a list of <see cref="IContainerProfile" />s that
    ///     are the direct parents of the specified profile.
    /// </returns>
    /// <typeparam name="TGroup">The type of each group to be returned.</typeparam>
    /// <typeparam name="TOrgUnit">The type of each organizational unit to be returned.</typeparam>
    /// <exception cref="ValidationException"><paramref name="profileId" /> is not valid.</exception>
    /// <exception cref="ArgumentException"><paramref name="profileId" /> is empty or contains only whitespace characters.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="profileId" /> is <c>null</c>.</exception>
    /// <exception cref="InstanceNotFoundException">No profile can be found considering its <paramref name="profileId" />.</exception>
    Task<IPaginatedList<IContainerProfile>> GetParentsOfProfileAsync<TGroup, TOrgUnit>(
        string profileId,
        RequestedProfileKind expectedKind,
        AssignmentQueryObject options = null,
        CancellationToken cancellationToken = default)
        where TGroup : GroupBasic
        where TOrgUnit : OrganizationBasic;

    /// <summary>
    ///     Returns recursive all parent profiles of requested container profile.
    /// </summary>
    /// <param name="profileId">The id of the user or group whose parents are to be returned.</param>
    /// <param name="expectedKind">The profile kind of each profile to be retrieved.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>
    ///     A task representing the asynchronous read operation. It wraps a list of <see cref="IContainerProfile" />s that
    ///     are the direct parents of the specified profile.
    /// </returns>
    /// <exception cref="ValidationException"><paramref name="profileId" /> is not valid.</exception>
    /// <exception cref="ArgumentException"><paramref name="profileId" /> is empty or contains only whitespace characters.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="profileId" /> is <c>null</c>.</exception>
    /// <exception cref="InstanceNotFoundException">No profile can be found considering its <paramref name="profileId" />.</exception>
    Task<List<IContainerProfile>> GetAllParentsOfProfileAsync(
        string profileId,
        RequestedProfileKind expectedKind,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Returns a list of profiles that are member of the specified container profile.
    ///     The expected kind of the parent can be defined in <paramref name="expectedParentType" />,
    ///     but this check can be deactivated by setting it to <see cref="ProfileContainerType.NotSpecified" />.
    /// </summary>
    /// <remarks>
    ///     All found instances of users,groups or organizational units will be converted to their estimated types<br />
    ///     (given by type parameters: <typeparamref name="TGroup" />, <typeparamref name="TUser" />,
    ///     <typeparamref name="TOrgUnit" />)
    /// </remarks>
    /// <param name="profileId">The id of the group whose children are to be returned.</param>
    /// <param name="expectedParentType">
    ///     The profile kind of the parent profile. If not set to
    ///     <see cref="ProfileContainerType.NotSpecified" />, the profile kind of the stored object will be checked to be of
    ///     the same kind.
    /// </param>
    /// <param name="expectedChildrenKind">The profile kind of each profile to be retrieved (optional parameter).</param>
    /// <param name="options">
    ///     Options to refine the search request and to set up pagination and sorting. If <c>null</c>, the
    ///     default values of pagination will be used.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>
    ///     A task representing the asynchronous read operation. It wraps a list of <see cref="IProfile" /> that are
    ///     groups or user.
    /// </returns>
    /// <typeparam name="TUser">The type of the user instances to be returned.</typeparam>
    /// <typeparam name="TGroup">The type of the group instances to be returned.</typeparam>
    /// <typeparam name="TOrgUnit">The type of the organizational unit instances to be returned.</typeparam>
    /// <exception cref="ValidationException"><paramref name="options" /> is not valid.</exception>
    /// <exception cref="ArgumentException"><paramref name="profileId" /> is empty or contains only whitespace characters.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="profileId" /> is <c>null</c>.</exception>
    /// <exception cref="InstanceNotFoundException">
    ///     No profile can be found considering its <paramref name="profileId" /> and
    ///     the expected parent type.
    /// </exception>
    Task<IPaginatedList<IProfile>> GetChildrenOfProfileAsync<TUser, TGroup, TOrgUnit>(
        string profileId,
        ProfileContainerType expectedParentType,
        RequestedProfileKind expectedChildrenKind = RequestedProfileKind.All,
        AssignmentQueryObject options = null,
        CancellationToken cancellationToken = default)
        where TUser : UserBasic
        where TGroup : GroupBasic
        where TOrgUnit : OrganizationBasic;
    
    /// <summary>
    ///     Get all functional rights of a specified profile.
    /// </summary>
    /// <param name="profileId">The id of the profile whose functional access rights are to be returned.</param>
    /// <param name="includeInherited">
    ///     A flag indicating whether inherited right objects will be analyzed or not. If false,
    ///     only direct assigned right object will be considered.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>A task representing the asynchronous read operation. It wraps a list of functional access rights.</returns>
    /// <exception cref="ArgumentException"><paramref name="profileId" /> is empty or contains only whitespace characters.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="profileId" /> is <c>null</c>.</exception>
    /// <exception cref="InstanceNotFoundException">No profile can be found considering its <paramref name="profileId" />.</exception>
    Task<IPaginatedList<string>> GetFunctionalAccessRightsOfProfileAsync(
        string profileId,
        bool includeInherited = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Checks whether <paramref name="functionalName" /> is part of profile's functional access rights.
    /// </summary>
    /// <param name="profileId">The id of the profile to be checked.</param>
    /// <param name="functionalName">The name of the functional access rights object to be checked.</param>
    /// <param name="includeInherited">
    ///     A flag indicating whether inherited right objects will be analyzed or not. If false,
    ///     only direct assigned right object will be considered.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>
    ///     A task representing the asynchronous read operation. It contains a boolean value that is <c>true</c> if the
    ///     profile owns <paramref name="functionalName" />, otherwise <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentException"><paramref name="profileId" /> is empty or contains only whitespace characters.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="profileId" /> is <c>null</c>.</exception>
    /// <exception cref="InstanceNotFoundException">No profile can be found considering its <paramref name="profileId" />.</exception>
    Task<bool> CheckFunctionalAccessRightOfProfileAsync(
        string profileId,
        string functionalName,
        bool includeInherited = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets the links of a profile.
    /// </summary>
    /// <param name="profileId">The id of the profile. whose links are to be returned.</param>
    /// <param name="options">
    ///     Options to set up pagination and sorting. If <c>null</c>, the default values of pagination will
    ///     be used.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>A task representing the asynchronous read operation. It wraps the found links of the profile.</returns>
    /// <exception cref="ArgumentException"><paramref name="profileId" /> is empty or contains only whitespace characters.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="profileId" /> is <c>null</c>.</exception>
    /// <exception cref="InstanceNotFoundException">No profile can be found considering its <paramref name="profileId" />.</exception>
    /// <exception cref="ValidationException"><paramref name="options" /> is not valid.</exception>
    Task<IPaginatedList<IAssignmentObject>> GetLinksForProfileAsync(
        string profileId,
        QueryObject options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retrieves the direct members (children) of a container profile asynchronously and returns them as a list of
    ///     <see cref="ConditionAssignment" />s.
    /// </summary>
    /// <param name="parentId">The unique identifier of the parent profile.</param>
    /// <param name="parentProfileKind">The kind of the parent profile (e.g., group).</param>
    /// <param name="memberIdFilter">Optional filter - only member ids including in this set will be returned, if it is not null or empty</param>
    /// <param name="cancellationToken">Optional cancellation token to cancel the operation.</param>
    /// <returns>
    ///     A task representing the asynchronous operation. The task result is a list of <see cref="ConditionAssignment" />.
    /// </returns>
    /// <remarks>
    ///     This method queries the data store to retrieve the direct members of the specified container profile.
    /// </remarks>
    /// <example>
    ///     <code>
    /// var members = await GetDirectMembersOfProfileAsync(parentId, ProfileKind.Group, cancellationToken);
    /// foreach (var member in members)
    /// {
    ///     // Process each member...
    /// }
    /// </code>
    /// </example>
    /// <exception cref="ArgumentNullException">If <paramref name="parentId" /> is <c>null</c></exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="parentId" /> is empty or contains only whitespaces <br />-or-<br />
    /// </exception>
    /// if
    /// <paramref name="parentProfileKind" />
    /// is not a container profile kind.
    /// <exception cref="InstanceNotFoundException">If parent entity could not be found.</exception>
    Task<IList<ConditionAssignment>> GetDirectMembersOfContainerProfileAsync(
        string parentId,
        ProfileKind parentProfileKind,
        IEnumerable<string> memberIdFilter = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets the assigned profiles dependent on a specified role or function.
    /// </summary>
    /// <param name="roleOrFunctionId">The id of the role or function that should restrict the result set.</param>
    /// <param name="options">
    ///     Options to refine the search request and to set up pagination and sorting. If <c>null</c>, the
    ///     default values of pagination will be used.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>
    ///     A task representing the asynchronous read operation. It wraps a list of <see cref="Member" />s that are
    ///     assigned to the object and the role or function.
    /// </returns>
    /// <exception cref="ValidationException">If <paramref name="options" /> is not valid.</exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="roleOrFunctionId" /> is empty or contains only whitespace
    ///     characters.
    /// </exception>
    /// <exception cref="ArgumentNullException"><paramref name="roleOrFunctionId" /> is <c>null</c>.</exception>
    /// <exception cref="InstanceNotFoundException">
    ///     No function or role can be found with key equals
    ///     <paramref name="roleOrFunctionId" />.
    /// </exception>
    Task<IPaginatedList<Member>> GetAssignedProfiles(
        string roleOrFunctionId,
        QueryObject options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets roles.
    /// </summary>
    /// <remarks>
    ///     All found instances of roles will be converted to their estimated types<br />
    ///     (given by <typeparamref name="TRole" />)
    /// </remarks>
    /// <typeparam name="TRole">
    ///     The type of each element in the result set (either <see cref="RoleBasic" /> or inherited from
    ///     <see cref="RoleBasic" />).
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
    ///     A task representing the asynchronous read operation. It wraps a list of <see cref="RoleBasic" />. If no roles
    ///     have been found, an empty list will be returned.
    /// </returns>
    /// <exception cref="ValidationException">If <paramref name="options" /> is not valid.</exception>
    Task<IPaginatedList<TRole>> GetRolesAsync<TRole>(
        QueryObject options = null,
        CancellationToken cancellationToken = default)
        where TRole : RoleBasic;

    /// <summary>
    ///     Gets a specified role.
    /// </summary>
    /// <param name="roleId">The id of the role to be returned.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>A task representing the asynchronous read operation. It wraps the requested <see cref="RoleView" />.</returns>
    /// <exception cref="ArgumentException"><paramref name="roleId" /> is empty or contains only whitespace characters.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="roleId" /> is <c>null</c>.</exception>
    /// <exception cref="InstanceNotFoundException">No role can be found with key equals <paramref name="roleId" />.</exception>
    Task<RoleView> GetRoleAsync(
        string roleId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Get all functions.
    /// </summary>
    /// <remarks>
    ///     All found instances of functions will be converted to their estimated types<br />
    ///     (given by <typeparamref name="TFunction" />)
    /// </remarks>
    /// <typeparam name="TFunction">
    ///     The type of each element in the result set (either <see cref="FunctionBasic" /> or
    ///     inherited from <see cref="FunctionBasic" />).
    /// </typeparam>
    /// <param name="options">
    ///     Options to refine the search request and to set up pagination and sorting. If <c>null</c>, the
    ///     default values of pagination will be used.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>
    ///     A task representing the asynchronous read operation. It wraps a list of found
    ///     <typeparamref name="TFunction" />s. If no functions have been found, an empty list will be returned.
    /// </returns>
    /// <exception cref="ValidationException">If <paramref name="options" /> is not valid.</exception>
    Task<IPaginatedList<TFunction>> GetFunctionsAsync<TFunction>(
        AssignmentQueryObject options = null,
        CancellationToken cancellationToken = default)
        where TFunction : FunctionBasic;

    /// <summary>
    ///     Get a specified function.
    /// </summary>
    /// <typeparam name="TFunction">
    ///     The type of each element in the result set (either <see cref="FunctionView" /> or inherited
    ///     from <see cref="FunctionView" />).
    /// </typeparam>
    /// <param name="functionId">An unique function id.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>A task representing the asynchronous read operation. It wraps the specified <see cref="FunctionView" />.</returns>
    /// <exception cref="ArgumentException"><paramref name="functionId" /> is empty or contains only whitespace characters.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="functionId" /> is <c>null</c>.</exception>
    /// <exception cref="InstanceNotFoundException">No function can be found whose key equals <paramref name="functionId" />.</exception>
    Task<TFunction> GetFunctionAsync<TFunction>(
        string functionId,
        CancellationToken cancellationToken = default)
        where TFunction : FunctionView;

    /// <summary>
    ///     Gets the tags of a profile or an object.
    /// </summary>
    /// <param name="profileOrObjectId">The profile or object id the tags should created for.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <param name="tagType"> The type of the tag. For for information see <see cref="RequestedTagType" />.</param>
    /// <returns>
    ///     A task representing the asynchronous read operation. It wraps a list of <see cref="CalculatedTag" />s of the
    ///     specified profile or object.
    /// </returns>
    /// <exception cref="ArgumentException">
    ///     <paramref name="profileOrObjectId" /> is empty or contains only whitespace
    ///     characters.
    /// </exception>
    /// <exception cref="ArgumentNullException"><paramref name="profileOrObjectId" /> is <c>null</c>.</exception>
    /// <exception cref="InstanceNotFoundException">
    ///     No function can be found whose key equals
    ///     <paramref name="profileOrObjectId" />.
    /// </exception>
    Task<IPaginatedList<CalculatedTag>> GetTagsOfProfileAsync(
        string profileOrObjectId,
        RequestedTagType tagType,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Returns the roles assigned to a specified profile (either user or group).
    /// </summary>
    /// <param name="profileId">The id of the user or group profile whose assigned roles should be retrieved.</param>
    /// <param name="options">
    ///     Options to refine the search request and to set up pagination and sorting. If <c>null</c>, the
    ///     default values of pagination will be used.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>
    ///     A task representing the asynchronous read operation. It wraps a list of roles with the applicable conditions.
    ///     If the specified profile has not assigned any roles, an empty list will be returned.
    /// </returns>
    /// <exception cref="ValidationException"><paramref name="options" /> is not valid.</exception>
    /// <exception cref="ArgumentException"><paramref name="profileId" /> is empty or contains only whitespace characters.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="profileId" /> is <c>null</c>.</exception>
    /// <exception cref="InstanceNotFoundException">No profile can be found considering its <paramref name="profileId" />.</exception>
    Task<IPaginatedList<LinkedRoleObject>> GetRolesOfProfileAsync(
        string profileId,
        AssignmentQueryObject options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Returns the functions assigned to a specified profile (either user or group).
    /// </summary>
    /// <param name="profileId">The id of the user or group profile whose assigned functions should be retrieved.</param>
    /// <param name="returnFunctionsRecursively">
    ///     States that the direct assigned functions should be returned (for a user), if false and recursively
    ///     assigned functions (for example from a group) otherwise
    /// </param>
    /// <param name="options">
    ///     Options to refine the search request and to set up pagination and sorting. If <c>null</c>, the
    ///     default values of pagination will be used.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>
    ///     A task representing the asynchronous read operation. It wraps a list of functions with the applicable
    ///     conditions. If the specified profile has not assigned any functions, an empty list will be returned.
    /// </returns>
    /// <exception cref="ValidationException"><paramref name="options" /> is not valid.</exception>
    /// <exception cref="ArgumentException"><paramref name="profileId" /> is empty or contains only whitespace characters.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="profileId" /> is <c>null</c>.</exception>
    /// <exception cref="InstanceNotFoundException">No profile can be found considering its <paramref name="profileId" />.</exception>
    Task<IPaginatedList<LinkedFunctionObject>> GetFunctionsOfProfileAsync(
        string profileId,
        bool returnFunctionsRecursively = false,
        AssignmentQueryObject options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Returns a JSON object that contains all settings of a profile with the specified config key.
    /// </summary>
    /// <param name="profileId">The id of the profile whose settings should be returned.</param>
    /// <param name="profileKind"></param>
    /// <param name="settingsKey">The key of the config that contains the requested settings.</param>
    /// <param name="includeInherited">Specifies whether values from parents are to be included. Defaults to <c>true</c>.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>
    ///     A task representing the asynchronous read operation. It wraps a <see cref="JObject" /> that represents the
    ///     requested settings. If no settings were found the value will be <c>null</c>.
    /// </returns>
    /// <exception cref="ArgumentException">
    ///     <paramref name="profileId" /> is empty or contains only whitespace characters.<br />-or-<br />
    ///     <paramref name="settingsKey" /> is empty or contains only whitespace characters.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="profileId" /> is <c>null</c>.<br />-or-<br />
    ///     <paramref name="settingsKey" /> is <c>null</c>.
    /// </exception>
    /// <exception cref="InstanceNotFoundException">No profile can be found considering its <paramref name="profileId" />.</exception>
    Task<JObject> GetSettingsOfProfileAsync(
        string profileId,
        ProfileKind profileKind,
        string settingsKey,
        bool includeInherited = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets all assignments for a profile.
    /// </summary>
    /// <param name="profileId">An unique id of the user profile.</param>
    /// <param name="includeInactiveAssignments"></param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>
    ///     A task representing the asynchronous read operation. It wraps a list of <see cref="ObjectIdent" />s
    ///     representing all assigned user ids.
    /// </returns>
    /// <exception cref="ArgumentException"><paramref name="profileId" /> is empty or contains only whitespace characters.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="profileId" /> is <c>null</c>.</exception>
    /// <exception cref="InstanceNotFoundException">No user profile can be found considering <paramref name="profileId" />.</exception>
    Task<IList<ObjectIdent>> GetAllAssignedIdsOfUserAsync(
        string profileId,
        bool includeInactiveAssignments,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Returns a list of tags.
    /// </summary>
    /// <param name="options">
    ///     Options to refine the search request and to set up pagination and sorting. If <c>null</c>, the
    ///     default values of pagination will be used.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>A task representing the asynchronous read operation. It wraps a list of found <see cref="Tag" />s.</returns>
    /// <exception cref="ValidationException"><paramref name="options" /> is not valid.</exception>
    Task<IPaginatedList<Tag>> GetTagsAsync(
        QueryObject options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Returns a list of tags for the given ids.
    /// </summary>
    /// <param name="tagIds">The list of ids to return the tags for.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>A task representing the asynchronous read operation. It wraps a list of found <see cref="Tag" />s.</returns>
    Task<IPaginatedList<Tag>> GetTagsAsync(
        IEnumerable<string> tagIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Returns the specific tag.
    /// </summary>
    /// <param name="tagId">The id of the user or group profile whose assigned functions should be retrieved.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>A task representing the asynchronous read operation. It wraps the requested <see cref="Tag" />.</returns>
    /// <exception cref="ArgumentException"><paramref name="tagId" /> is empty or contains only whitespace characters.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="tagId" /> is <c>null</c>.</exception>
    /// <exception cref="InstanceNotFoundException">No user profile can be found considering <paramref name="tagId" />.</exception>
    Task<Tag> GetTagAsync(
        string tagId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Checks if tags are already stored in the repository using their ids.<br />
    ///     The method will return all tag ids, that could be found in repository.
    /// </summary>
    /// <param name="tagIds">A sequence of tag ids that should be checked.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>
    ///     A task representing the asynchronous read operation. It wraps a list of strings that represent tag ids of
    ///     existent tag objects..
    /// </returns>
    /// <exception cref="ArgumentException"><paramref name="tagIds" /> is empty or contains only empty strings/null values.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="tagIds" /> is <c>null</c>.</exception>
    Task<IEnumerable<string>>
        GetExistentTagsAsync(
            IEnumerable<string> tagIds,
            CancellationToken cancellationToken = default);

    /// <summary>
    ///     Returns all profiles without any enhanced filter and pagination settings.
    /// </summary>
    /// <remarks>The <c>view</c> version of profiles will be returned.</remarks>
    /// <param name="profileKindFilter">
    ///     The only filter that can be used. It limits the kind of profiles returned by this
    ///     method.
    /// </param>
    /// <param name="sortingPropertyName">The name of the property that will be used for sorting. Default value: id</param>
    /// <param name="sortOrder">The order (direction) of the sorting done by this method.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>
    ///     A task representing the asynchronous read operation. It wraps the requested collection of
    ///     <see cref="IProfile" />s.
    /// </returns>
    Task<IList<IProfile>> GetAllProfilesAsync(
        RequestedProfileKind profileKindFilter = RequestedProfileKind.All,
        string sortingPropertyName = "id",
        SortOrder sortOrder = SortOrder.Asc,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Returns all assignment objects (i.e. roles, functions) without any enhanced filter and pagination settings.
    /// </summary>
    /// <remarks>The <c>view</c> version of objects will be returned.</remarks>
    /// <param name="typeFilter">
    ///     The only filter that can be used. It limits the kind of assignment objects returned by this
    ///     method.
    /// </param>
    /// <param name="sortingPropertyName">The name of the property that will be used for sorting. Default value: id</param>
    /// <param name="sortOrder">The order (direction) of the sorting done by this method.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>
    ///     A task representing the asynchronous read operation. It wraps the requested collection of
    ///     <see cref="IAssignmentObject" />s.
    /// </returns>
    Task<IList<IAssignmentObject>> GetAllAssignmentObjectsAsync(
        RequestedAssignmentObjectType typeFilter = RequestedAssignmentObjectType.All,
        string sortingPropertyName = "id",
        SortOrder sortOrder = SortOrder.Asc,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    ///     Returns an  <see cref="IProfile" />  regarding the specified <paramref name="profileId" /> and the
    ///     optional parameter <paramref name="allowExternalIds" /> and <paramref name="source" />.
    /// </summary>
    /// <param name="profileId">
    ///     The profileId identifies the profile by its id, if the
    ///     <paramref name="allowExternalIds" /> uses the default value true.
    /// </param>
    /// <param name="allowExternalIds">
    ///     The parameter has as default true. If true only the external id property must match,
    ///     otherwise the "own" id property.
    /// </param>
    /// <param name="source">Specifies that the profile should match a special source.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>
    ///     A task representing the asynchronous read operation. It wraps a <see cref="IProfile" />.
    ///     A <see cref="IProfile" /> can be a <see cref="Organization" />, <see cref="Group" /> or an <see cref="User" />.
    /// </returns>
    Task<List<IProfile>> GetProfileByExternalOrInternalIdAsync<TUser, TGroup, TOrgUnit>(
        string profileId,
        bool allowExternalIds = true,
        string source = null,
        CancellationToken cancellationToken = default)
        where TUser : User
        where TGroup : Group
        where TOrgUnit : Organization;
}
