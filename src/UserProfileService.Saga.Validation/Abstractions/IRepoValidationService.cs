using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.BasicModels;
using Maverick.UserProfileService.Models.Models;
using Maverick.UserProfileService.Models.RequestModels;
using Newtonsoft.Json.Linq;
using UserProfileService.Validation.Abstractions;

namespace UserProfileService.Saga.Validation.Abstractions;

/// <summary>
///     Describes a service that validates the given entities with information from the database.
/// </summary>
internal interface IRepoValidationService
{
    /// <summary>
    ///     Checks if a function with the same organization and role exists.
    /// </summary>
    /// <param name="roleId">Role id of the function.</param>
    /// <param name="organizationId">Organization id of the function.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>The result of the validation as <see cref="ValidationResult" />.</returns>
    public Task<ValidationResult> ValidateDuplicateFunctionAsync(
        string roleId,
        string organizationId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Checks if a role with the given id exists.
    /// </summary>
    /// <param name="roleId">Id of the role to check.</param>
    /// <param name="member">The name of the property to which the <see cref="ValidationAttribute" /> is assigned.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>The result of the validation as <see cref="ValidationResult" />.</returns>
    public Task<ValidationResult<RoleBasic>> ValidateRoleExistsAsync(
        string roleId,
        string member = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Checks if a organization with the given id exists.
    /// </summary>
    /// <param name="organizationId">Id of the organization to check.</param>
    /// <param name="member">The name of the property to which the <see cref="ValidationAttribute" /> is assigned.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>The result of the validation as <see cref="ValidationResult" />.</returns>
    public Task<ValidationResult> ValidateOrganizationExistsAsync(
        string organizationId,
        string member = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Checks if a function with the given id exists.
    /// </summary>
    /// <param name="functionId">Id of the function to check.</param>
    /// <param name="member">The name of the property to which the <see cref="ValidationAttribute" /> is assigned.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>The result of the validation as <see cref="ValidationResult" />.</returns>
    public Task<ValidationResult> ValidateFunctionExistsAsync(
        string functionId,
        string member = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Checks if a tag with the given id exists.
    /// </summary>
    /// <param name="tagId">Id of the tag to check.</param>
    /// <param name="member">The name of the property to which the <see cref="ValidationAttribute" /> is assigned.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>The result of the validation as <see cref="ValidationResult" />.</returns>
    public Task<ValidationResult<Tag>> ValidateTagExistsAsync(
        string tagId,
        string member = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Checks if the list of given ids of tags exists.
    /// </summary>
    /// <param name="tagIds">List of tag ids to check.</param>
    /// <param name="member">The name of the property to which the <see cref="ValidationAttribute" /> is assigned.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>The result of the validation as <see cref="ValidationResult" />.</returns>
    public Task<ValidationResult> ValidateTagsExistAsync(
        ICollection<string> tagIds,
        string member = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Checks if a group with the name and display name already exists.
    ///     Not only the name is compared with the name, but also the name with the display name.
    ///     The combination must be unique.
    /// </summary>
    /// <param name="name">Name of the group to check.</param>
    /// <param name="displayName">Display name of the group to check.</param>
    /// <param name="groupId">The id of the group to be ignored when checking the names.</param>
    /// <param name="ignoreCase">Specifies whether upper and lower case are ignored during validation.</param>
    /// <param name="memberName">The name of the name property to which the <see cref="ValidationAttribute" /> is assigned.</param>
    /// <param name="memberDisplayName">
    ///     The name of the display name property to which the <see cref="ValidationAttribute" />
    ///     is assigned.
    /// </param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>The result of the validation as <see cref="ValidationResult" />.</returns>
    public Task<ValidationResult> ValidateGroupExistsAsync(
        string name,
        string displayName,
        string groupId = null,
        bool ignoreCase = true,
        string memberName = null,
        string memberDisplayName = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Check if a user with the given email exists.
    /// </summary>
    /// <param name="email">Email to check.</param>
    /// <param name="ignoredId">Users with given email and this id will be ignored.</param>
    /// <param name="member">The name of the property to which the <see cref="ValidationAttribute" /> is assigned.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>The result of the validation as <see cref="ValidationResult" />.</returns>
    public Task<ValidationResult> ValidateUserEmailExistsAsync(
        string email,
        string ignoredId,
        string member = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Check if a object with the given <see cref="ObjectIdent" /> exists.
    /// </summary>
    /// <param name="objectIdent">Identifier of object to check.</param>
    /// <param name="member">The name of the property to which the <see cref="ValidationAttribute" /> is assigned.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>The result of the validation as <see cref="ValidationResult" />.</returns>
    public Task<ValidationResult> ValidateObjectExistsAsync(
        IObjectIdent objectIdent,
        string member = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Check if a profile with the given <see cref="ProfileIdent" /> exists.
    /// </summary>
    /// <param name="profileIdent">Identifier of profile to check.</param>
    /// <param name="member">The name of the property to which the <see cref="ValidationAttribute" /> is assigned.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>The result of the validation as <see cref="ValidationResult" />.</returns>
    public Task<ValidationResult<IProfile>> ValidateProfileExistsAsync(
        ProfileIdent profileIdent,
        string member = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Check if the list of the given <see cref="ObjectIdent" /> exists.
    /// </summary>
    /// <param name="objectIdents">List of <see cref="ObjectIdent" /> to check.</param>
    /// <param name="member">The name of the property to which the <see cref="ValidationAttribute" /> is assigned.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>The result of the validation as <see cref="ValidationResult" />.</returns>
    public Task<ValidationResult> ValidateObjectsExistAsync(
        ICollection<IObjectIdent> objectIdents,
        string member = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Check if the client settings of the given settings key for the given profile exists.
    /// </summary>
    /// <param name="profile">Profile for which to check if the client settings exist.</param>
    /// <param name="key">Key of the client settings to check.</param>
    /// <param name="member">The name of the property to which the <see cref="ValidationAttribute" /> is assigned.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>The result of the validation as <see cref="ValidationResult" />.</returns>
    public Task<ValidationResult<JObject>> ValidateClientSettingsExistsAsync(
        ProfileIdent profile,
        string key,
        string member = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Validates the assignments between the container profiles to ensure that a directed graph is built and not a
    ///     undirected graph.
    /// </summary>
    /// <param name="objectIdent">Target object the assignment are assigned to or unassigned from. </param>
    /// <param name="assignments">Source object to assign to or unassign from target object.</param>
    /// <param name="member">Member name the validation result belongs to.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>A tuple consisting whether an error exists and the corresponding error.</returns>
    Task<ValidationResult> ValidateContainerProfileAssignmentGraphAsync(
        IObjectIdent objectIdent,
        ICollection<ConditionObjectIdent> assignments,
        string member = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Checks whether there are connections to functions for the role.
    /// </summary>
    /// <param name="id">Id of the role to check.</param>
    /// <param name="member">Member name the validation result belongs to.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>A tuple consisting whether an error exists and the corresponding error.</returns>
    Task<ValidationResult> ValidateRoleAssignmentsAsync(
        string id,
        string member = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Asynchronously validates the existence of assignments for a specified object.
    /// </summary>
    /// <param name="objectIdent">The object identifier for which assignments are being validated.</param>
    /// <param name="assignments">The list of assignment references to validate for existence.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation (optional).</param>
    /// <returns>
    ///     A task representing the asynchronous operation. The task result is a <see cref="ValidationResult" /> indicating
    ///     whether the assignments exist for the specified object.
    /// </returns>
    Task<ValidationResult> ValidateAssignmentsExistAsync(
        IObjectIdent objectIdent,
        IList<ConditionObjectIdent> assignments,
        CancellationToken cancellationToken = default);
}
