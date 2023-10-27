using Maverick.UserProfileService.Models.BasicModels;

namespace UserProfileService.Validation.Abstractions.Configuration;

/// <summary>
///     Validation configuration for <see cref="UserBasic" />
/// </summary>
public class UserValidationConfiguration
{
    /// <summary>
    ///     Specifies whether the duplicate check is performed for email address.
    /// </summary>
    public bool DuplicateEmailAllowed { get; set; } = true;
}
