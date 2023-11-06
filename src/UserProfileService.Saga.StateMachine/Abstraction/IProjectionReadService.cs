using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.BasicModels;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using Maverick.UserProfileService.Models.RequestModels;
using Newtonsoft.Json.Linq;
using UserProfileService.Common.V2.Exceptions;

namespace UserProfileService.StateMachine.Abstraction;

/// <summary>
///     Defines the service to read projection models.
/// </summary>
public interface IProjectionReadService
{
    /// <summary>
    ///     Returns all direct parents of a profile.
    /// </summary>
    /// <param name="id">Identifier of profile to get direct parents of.</param>
    /// <returns>Collection of ids of direct parents.</returns>
    public Task<ICollection<ProfileIdent>> GetParentsOfProfileAsync(string id);

    /// <summary>
    ///     Returns all children of the given profile.
    /// </summary>
    /// <param name="id">Identifier of profile to get all children of.</param>
    /// <param name="profileKind">Kind of profile to get children of.</param>
    /// <returns>Collection of <see cref="ProfileIdent" /> of all children.</returns>
    public Task<ICollection<ProfileIdent>> GetChildrenOfProfileAsync(string id, ProfileKind profileKind);

    /// <summary>
    ///     Gets the assigned profiles dependent on a specified role or function.
    /// </summary>
    /// <param name="roleOrFunctionId">The id of the role or function that should restrict the result set.</param>
    /// <returns>
    ///     A task representing the asynchronous read operation. It wraps a list of <see cref="Member" />s that are
    ///     assigned to the object and the role or function.
    /// </returns>
    /// <exception cref="ArgumentException">
    ///     <paramref name="roleOrFunctionId" /> is empty or contains only whitespace
    ///     characters.
    /// </exception>
    /// <exception cref="ArgumentNullException"><paramref name="roleOrFunctionId" /> is <c>null</c>.</exception>
    /// <exception cref="InstanceNotFoundException">
    ///     No function or role can be found with key equals
    ///     <paramref name="roleOrFunctionId" />.
    /// </exception>
    public Task<ICollection<Member>> GetAssignedProfilesAsync(string roleOrFunctionId);

    /// <summary>
    ///     Get the tag from a repository.
    /// </summary>
    /// <param name="id">Identifier of tag to return.</param>
    /// <returns>Tag for given identifier.</returns>
    public Task<Tag> GetTagAsync(string id);

    /// <summary>
    ///     Get the role from a repository.
    /// </summary>
    /// <param name="id">Identifier of role to return.</param>
    /// <returns>Role for given identifier.</returns>
    public Task<RoleBasic> GetRoleAsync(string id);

    /// <summary>
    ///     Get the profile from a repository.
    /// </summary>
    /// <param name="id">Identifier of profile to return.</param>
    /// <param name="profileKind">The profile kind of the profile to be retrieved.</param>
    /// <returns>Profile for given identifier.</returns>
    public Task<IProfile> GetProfileAsync(string id, ProfileKind profileKind);

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
}
