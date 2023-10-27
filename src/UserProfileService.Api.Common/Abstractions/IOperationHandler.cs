using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using Maverick.UserProfileService.Models.Modifiable;
using Maverick.UserProfileService.Models.RequestModels;
using Microsoft.AspNetCore.JsonPatch;
using Newtonsoft.Json.Linq;

namespace UserProfileService.Api.Common.Abstractions;

/// <summary>
///     This interface provides all methods that are related to profiles (Users, Groups).
///     The normal CUD (Create, Update, Delete) operations are defined here. Also all assignment and discharge operation that
///     are
///     related to profiles. The custom properties are also part of this interface due the profile can
///     include one or more custom property.
/// </summary>
public interface IOperationHandler
{
    /// <summary>
    ///     Creates a user profile.
    /// </summary>
    /// <param name="user">The profile that should be created.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>An Id related to an ticket representing the operation.</returns>
    Task<string> CreateUserProfileAsync(
        CreateUserRequest user,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates a group profile.
    /// </summary>
    /// <param name="group">The profile that should be created.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>An Id related to an ticket representing the operation.</returns>
    Task<string> CreateGroupProfileAsync(
        CreateGroupRequest group,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates a organization profile.
    /// </summary>
    /// <param name="organization">The profile that should be created.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>An Id related to an ticket representing the operation.</returns>
    Task<string> CreateOrganizationProfileAsync(
        CreateOrganizationRequest organization,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates a tag.
    /// </summary>
    /// <param name="tag">The tag that should be created.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>An Id related to an ticket representing the operation.</returns>
    Task<string> CreateTagAsync(
        CreateTagRequest tag,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Updates an single user profile.
    /// </summary>
    /// <param name="profileId">The id of the profile to be updated.</param>
    /// <param name="profile">The profile to be updated.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>An Id related to an ticket representing the operation.</returns>
    Task<string> UpdateUserProfileAsync(
        string profileId,
        UserModifiableProperties profile,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Updates an single group profile.
    /// </summary>
    /// <param name="profileId">The id of the profile to be updated.</param>
    /// <param name="profile">The profile to be updated.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>An Id related to an ticket representing the operation.</returns>
    Task<string> UpdateGroupProfileAsync(
        string profileId,
        GroupModifiableProperties profile,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Updates an single organization profile.
    /// </summary>
    /// <param name="profileId">The id of the profile to be updated.</param>
    /// <param name="profile">The profile to be updated.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>An Id related to an ticket representing the operation.</returns>
    Task<string> UpdateOrganizationProfileAsync(
        string profileId,
        OrganizationModifiableProperties profile,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Delete a single group.
    /// </summary>
    /// <param name="id">The group to be deleted.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>An Id related to an ticket representing the operation.</returns>
    Task<string> DeleteGroupAsync(
        string id,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Delete a single organization.
    /// </summary>
    /// <param name="id">The organization to be deleted.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>An Id related to an ticket representing the operation.</returns>
    Task<string> DeleteOrganizationAsync(
        string id,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Delete a single tag.
    /// </summary>
    /// <param name="id">The tag to be deleted.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>An Id related to an ticket representing the operation.</returns>
    Task<string> DeleteTagAsync(
        string id,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Delete a single user.
    /// </summary>
    /// <param name="id">The user to be deleted.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>An Id related to an ticket representing the operation.</returns>
    Task<string> DeleteUserAsync(
        string id,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Create assignments for a specified profile with objects by a role or a function.
    /// </summary>
    /// <param name="profileId">The id of the profile to assign.</param>
    /// <param name="profileKind">Profile kind the object is assigned to.</param>
    /// <param name="functionId">The function id the objects should be connected through.</param>
    /// <param name="conditions">Condition when the assignment is valid.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>An Id related to an ticket representing the operation.</returns>
    Task<string> AddProfileToRoleAsync(
        string profileId,
        ProfileKind profileKind,
        string functionId,
        RangeCondition[] conditions,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Update the profile assignments for the specified role.
    /// </summary>
    /// <param name="roleId">The role id the profiles should be added to or removed from.</param>
    /// <param name="assignments">Condition when the assignment is valid.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>An Id related to an ticket representing the operation.</returns>
    Task<string> UpdateProfileToRoleAssignmentsAsync(
        string roleId,
        BatchAssignmentRequest assignments,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Create assignments for a specified profile with objects by a role or a function.
    /// </summary>
    /// <param name="profileId">The id of the profile to assign.</param>
    /// <param name="profileKind">Profile kind the object is assigned to.</param>
    /// <param name="functionId">The function id the objects should be connected through.</param>
    /// <param name="conditions">Condition when the assignment is valid.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>An Id related to an ticket representing the operation.</returns>
    Task<string> AddProfileToFunctionAsync(
        string profileId,
        ProfileKind profileKind,
        string functionId,
        RangeCondition[] conditions,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Update the profile assignments for the specified function.
    /// </summary>
    /// <param name="functionId">The function id the profiles should be added to or removed from.</param>
    /// <param name="assignments">Condition when the assignment is valid.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>An Id related to an ticket representing the operation.</returns>
    Task<string> UpdateProfileToFunctionAssignmentsAsync(
        string functionId,
        BatchAssignmentRequest assignments,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Delete assignments for a specified profile with objects by a role or a function.
    /// </summary>
    /// <param name="profileId">The id of the profile to be unassigned from specified objects.</param>
    /// <param name="profileKind">Profile kind the object is assigned to.</param>
    /// <param name="roleOrFunctionId">The role or function id the objects should be unassigned from.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>An Id related to an ticket representing the operation.</returns>
    Task<string> RemoveProfileFromRoleAsync(
        string profileId,
        ProfileKind profileKind,
        string roleOrFunctionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Delete assignments for a specified profile with objects by a role or a function.
    /// </summary>
    /// <param name="profileId">The id of the profile to be unassigned from specified objects.</param>
    /// <param name="profileKind">Profile kind the object is assigned to.</param>
    /// <param name="roleOrFunctionId">The role or function id the objects should be unassigned from.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>An Id related to an ticket representing the operation.</returns>
    Task<string> RemoveProfileFromFunctionAsync(
        string profileId,
        ProfileKind profileKind,
        string roleOrFunctionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Assign profiles to a container profile. The assigned profiles can be groups, organizations or users.
    /// </summary>
    /// <param name="profiles">The profiles to be assigned to a container profile.</param>
    /// <param name="containerProfile">The container profile id should be assigned to the profiles.</param>
    /// <param name="conditions">Condition when the assignment is valid.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>An Id related to an ticket representing the operation.</returns>
    Task<string> AssignProfilesToContainerProfileAsync(
        ProfileIdent[] profiles,
        ProfileIdent containerProfile,
        RangeCondition[] conditions,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Assign profiles to a container profile. The assigned profiles can be groups, organizations or users.
    /// </summary>
    /// <param name="containerProfile">The container profile id should be assigned to the profiles.</param>
    /// <param name="assignments">Assignments of container profile to update.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>An Id related to an ticket representing the operation.</returns>
    Task<string> UpdateProfilesToContainerProfileAssignmentsAsync(
        ProfileIdent containerProfile,
        BatchAssignmentRequest assignments,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Discharges profiles from a container profile. The assigned profiles can be groups, organizations or users.
    /// </summary>
    /// <param name="profiles">The profiles to be unassigned from a container profile.</param>
    /// <param name="containerProfile">The container profile id should be unassigned from the profiles.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>An Id related to an ticket representing the operation.</returns>
    Task<string> RemoveProfileAssignmentsFromContainerProfileAsync(
        ProfileIdent[] profiles,
        ProfileIdent containerProfile,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Update the role assignments for the specified user.
    /// </summary>
    /// <param name="userId">The user id the roles should be added to or removed from.</param>
    /// <param name="assignments">The assignments to update.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>An Id related to an ticket representing the operation.</returns>
    Task<string> UpdateRoleToUserAssignmentsAsync(
        string userId,
        BatchAssignmentRequest assignments,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Update the function assignments for the specified user.
    /// </summary>
    /// <param name="userId">The user id the functions should be added to or removed from.</param>
    /// <param name="assignments">The assignments to update.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>An Id related to an ticket representing the operation.</returns>
    Task<string> UpdateFunctionToUserAssignmentsAsync(
        string userId,
        BatchAssignmentRequest assignments,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Update the container profile assignments for the specified user.
    /// </summary>
    /// <param name="userId">The user id the functions should be added to or removed from.</param>
    /// <param name="assignments">The assignments to update.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>An Id related to an ticket representing the operation.</returns>
    Task<string> UpdateContainerProfileToUserAssignmentsAsync(
        string userId,
        BatchAssignmentRequest assignments,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates a role.
    /// </summary>
    /// <param name="role">The role to be created.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>An Id related to an ticket representing the operation.</returns>
    Task<string> CreateRoleAsync(
        CreateRoleRequest role,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Updates an existing role.
    /// </summary>
    /// <param name="roleId">The roleId of the role to update.</param>
    /// <param name="roleToUpdate">The new role properties to update.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>An Id related to an ticket representing the operation.</returns>
    Task<string> UpdateRoleAsync(
        string roleId,
        RoleModifiableProperties roleToUpdate,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes an existing role.
    /// </summary>
    /// <param name="roleId">The role id to be deleted.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>An Id related to an ticket representing the operation.</returns>
    Task<string> DeleteRoleAsync(
        string roleId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates a new function.
    /// </summary>
    /// <param name="function">The function that should be created.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>An Id related to an ticket representing the operation.</returns>
    Task<string> CreateFunctionAsync(
        CreateFunctionRequest function,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes an existing function.
    /// </summary>
    /// <param name="functionId">The if of the function that should be deleted.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>An Id related to an ticket representing the operation.</returns>
    Task<string> DeleteFunctionAsync(
        string functionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Create tags for a specified profile.
    /// </summary>
    /// <param name="profileId">The profile id the tags should created for.</param>
    /// <param name="profileKind">Profile kind the object is assigned to.</param>
    /// <param name="tags">The tags to be created.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>An Id related to an ticket representing the operation.</returns>
    Task<string> CreateProfileTagsAsync(
        string profileId,
        ProfileKind profileKind,
        TagAssignment[] tags,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Create tags for a specified role.
    /// </summary>
    /// <param name="roleId">The role id the tags should created for.</param>
    /// <param name="tags">The tags to be created.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>An Id related to an ticket representing the operation.</returns>
    Task<string> CreateRoleTagsAsync(
        string roleId,
        TagAssignment[] tags,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Create tags for a specified function.
    /// </summary>
    /// <param name="functionId">The function id the tags should created for.</param>
    /// <param name="tags">The tags to be created.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>An Id related to an ticket representing the operation.</returns>
    Task<string> CreateFunctionTagsAsync(
        string functionId,
        TagAssignment[] tags,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Delete tags for a specified profile.
    /// </summary>
    /// <param name="profileId">The profile id the the tags should deleted for.</param>
    /// <param name="profileKind">Profile kind the object is assigned to.</param>
    /// <param name="tagIds">The id of the tags to be deleted.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>An Id related to an ticket representing the operation.</returns>
    Task<string> DeleteTagsFromProfile(
        string profileId,
        ProfileKind profileKind,
        string[] tagIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Delete tags for a specified role.
    /// </summary>
    /// <param name="roleId">The role id the the tags should deleted for.</param>
    /// <param name="tagIds">The id of the tags to be deleted.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>An Id related to an ticket representing the operation.</returns>
    Task<string> DeleteTagsFromRole(
        string roleId,
        string[] tagIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Delete tags for a specified function.
    /// </summary>
    /// <param name="functionId">The function id the the tags should deleted for.</param>
    /// <param name="tagIds">The id of the tags to be deleted.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>An Id related to an ticket representing the operation.</returns>
    Task<string> DeleteTagsFromFunction(
        string functionId,
        string[] tagIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Sets the configuration for a profile.
    /// </summary>
    /// <param name="profileId">The profile id.</param>
    /// <param name="profileKind">The profile kind.</param>
    /// <param name="configKey">The configuration identifier.</param>
    /// <param name="configurationObject">The configuration.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>An Id related to an ticket representing the operation.</returns>
    Task<string> SetProfileConfiguration(
        string profileId,
        ProfileKind profileKind,
        string configKey,
        JObject configurationObject,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Sets the configuration for profiles.
    /// </summary>
    /// <param name="configKey">The configuration identifier.</param>
    /// <param name="configRequest">The request to set configuration for multiple profiles.</param>
    /// <param name="profileKind">Profile kind of profiles.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>An Id related to an ticket representing the operation.</returns>
    Task<string> SetProfilesConfiguration(
        string configKey,
        BatchConfigSettingsRequest configRequest,
        ProfileKind profileKind,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Updates the configuration for a profile.
    /// </summary>
    /// <param name="profileId">The profile id.</param>
    /// <param name="profileKind">The profile kind.</param>
    /// <param name="configKey">The configuration identifier.</param>
    /// <param name="configurationObject">The configuration.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>An Id related to an ticket representing the operation.</returns>
    Task<string> UpdateProfileConfiguration(
        string profileId,
        ProfileKind profileKind,
        string configKey,
        JsonPatchDocument configurationObject,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Removes a configuration from a profile
    /// </summary>
    /// <param name="profileId">The profile id.</param>
    /// <param name="profileKind">The profile kind.</param>
    /// <param name="configKey">The configuration identifier.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>An Id related to an ticket representing the operation.</returns>
    Task<string> RemoveProfileConfiguration(
        string profileId,
        ProfileKind profileKind,
        string configKey,
        CancellationToken cancellationToken = default);
}
