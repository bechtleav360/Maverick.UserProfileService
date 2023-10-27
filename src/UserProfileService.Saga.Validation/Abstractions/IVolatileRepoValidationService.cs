using System.Threading;
using System.Threading.Tasks;
using Maverick.UserProfileService.Models.Models;
using UserProfileService.Validation.Abstractions;

namespace UserProfileService.Saga.Validation.Abstractions;

/// <summary>
///     Describes a service that validates the given entities with information from the database in scope of volatile data
///     sets.
/// </summary>
public interface IVolatileRepoValidationService
{
    /// <summary>
    ///     Checks whether the user settings object inside the provided section exists or not.
    /// </summary>
    /// <param name="userId">Identifier of the user profile to check.</param>
    /// <param name="objectId">The id of the object to check if existent.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <param name="sectionName">The name of the section the object to check is part of.</param>
    /// <returns>A tuple consisting whether an error exists and the corresponding error.</returns>
    Task<ValidationResult> ValidateUserSettingObjectExistsAsync(
        string userId,
        string sectionName,
        string objectId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Check if a profile with the given <see cref="ProfileIdent" /> exists.
    /// </summary>
    /// <param name="userId">Identifier of the user profile to check.</param>
    /// <param name="member">The name of the property to which the <see cref="ValidationAttribute" /> is assigned.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>The result of the validation as <see cref="ValidationResult" />.</returns>
    public Task<ValidationResult> ValidateProfileExistsAsync(
        string userId,
        string member = null,
        CancellationToken cancellationToken = default);
}
