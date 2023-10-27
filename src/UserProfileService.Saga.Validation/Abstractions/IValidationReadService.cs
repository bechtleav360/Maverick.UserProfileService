using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.BasicModels;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using Maverick.UserProfileService.Models.RequestModels;
using Newtonsoft.Json.Linq;
using UserProfileService.Common.V2.Exceptions;

namespace UserProfileService.Saga.Validation.Abstractions;

/// <summary>
///     Describes a service that provides the read operations for validation.
/// </summary>
public interface IValidationReadService
{
    /// <summary>
    ///     Checks whether the profile with the given id and kind exists.
    /// </summary>
    /// <param name="id">Identifier of profile.</param>
    /// <param name="profileKind">The profile kind of the profile to be retrieved.</param>
    /// <returns>True if profile exists, otherwise false.</returns>
    public Task<bool> CheckProfileExistsAsync(string id, ProfileKind profileKind);

    /// <summary>
    ///     Checks whether the object with the given ident exists.
    /// </summary>
    /// <param name="objectIdent">Identifier of object.</param>
    /// <returns>True if object exists, otherwise false.</returns>
    public Task<bool> CheckObjectExistsAsync(IObjectIdent objectIdent);

    /// <summary>
    ///     Checks whether the tag with the given id exists.
    /// </summary>
    /// <param name="ids">Identifier of tags.</param>
    /// <returns>True if tag exists, otherwise false.</returns>
    public Task<IDictionary<string, bool>> CheckTagsExistAsync(params string[] ids);

    /// <summary>
    ///     Checks whether a user with the given email already exists.
    /// </summary>
    /// <param name="email">Email to be checked.</param>
    /// <param name="userId">User id to use to ignore while checking email.</param>
    /// <returns>True if email is already in use, otherwise false.</returns>
    public Task<bool> CheckUserEmailExistsAsync(string email, string userId = "");

    /// <summary>
    ///     Checks whether a group with the given (display) name already exists.
    /// </summary>
    /// <param name="name">Name to be checked.</param>
    /// <param name="displayName">Display name to be checked.</param>
    /// <param name="ignoreCase">Specifies if check should be ignore case.</param>
    /// <param name="groupId">Group id to use to ignore while checking (display) name.</param>
    /// <returns>True if (display) name is already in use, otherwise false.</returns>
    public Task<bool> CheckGroupNameExistsAsync(
        string name,
        string displayName,
        bool ignoreCase,
        string groupId = "");

    /// <summary>
    ///     Get the profile from a repository.
    /// </summary>
    /// <param name="id">Identifier of profile to return.</param>
    /// <param name="profileKind">The profile kind of the profile to be retrieved.</param>
    /// <returns>Profile for given identifier.</returns>
    public Task<IProfile> GetProfileAsync(string id, ProfileKind profileKind);

    /// <summary>
    ///     Get the profiles from a repository.
    /// </summary>
    /// <param name="ids">Identifiers of profiles to return.</param>
    /// <param name="profileKind">The profile kind of the profiles to be retrieved.</param>
    /// <returns>Profiles for given identifiers.</returns>
    public Task<ICollection<IProfile>> GetProfilesAsync(ICollection<string> ids, ProfileKind profileKind);

    /// <summary>
    ///     Get the function from a repository.
    /// </summary>
    /// <param name="id">Identifier of function to return.</param>
    /// <returns>Function for given identifier.</returns>
    public Task<FunctionView> GetFunctionAsync(string id);

    /// <summary>
    ///     Get the functions from a repository by given parameter filters.
    /// </summary>
    /// <param name="roleId">Identifier of role to which the functions are assigned.</param>
    /// <param name="organizationId">Identifier of organization to which the functions are assigned.</param>
    /// <returns>Functions for given role and tag filters.</returns>
    public Task<ICollection<FunctionBasic>> GetFunctionsAsync(string roleId, string organizationId);

    /// <summary>
    ///     Get the role from a repository.
    /// </summary>
    /// <param name="id">Identifier of role to return.</param>
    /// <returns>Role for given identifier.</returns>
    public Task<RoleBasic> GetRoleAsync(string id);

    /// <summary>
    ///     Get the tag from a repository.
    /// </summary>
    /// <param name="id">Identifier of tag to return.</param>
    /// <returns>Tag for given identifier.</returns>
    public Task<Tag> GetTagAsync(string id);

    /// <summary>
    ///     Get all role assigned function ids.
    /// </summary>
    /// <param name="roleId">Identifier of role to be checked.</param>
    /// <returns>Identifiers of assigned functions.</returns>
    public Task<string[]> GetRoleFunctionAssignmentsAsync(string roleId);

    /// <summary>
    ///     Returns recursive all parents of a profile.
    /// </summary>
    /// <param name="id">Identifier of profile to get all parents of.</param>
    /// <returns>Collection of ids of all parents.</returns>
    public Task<string[]> GetAllParentsOfProfile(string id);

    /// <summary>
    ///     Returns a JSON object that contains all settings of a profile with the specified config key.
    /// </summary>
    /// <param name="profileId">The id of the profile whose settings should be returned.</param>
    /// <param name="profileKind"></param>
    /// <param name="settingsKey">The key of the config that contains the requested settings.</param>
    /// <returns>
    ///     A task representing the asynchronous read operation. It wraps a <see cref="JObject" /> that represents the
    ///     requested settings.
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
    public Task<JObject> GetSettingsOfProfileAsync(string profileId, ProfileKind profileKind, string settingsKey);
}
