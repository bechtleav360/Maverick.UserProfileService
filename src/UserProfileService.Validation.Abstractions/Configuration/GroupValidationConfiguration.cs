using Maverick.UserProfileService.Models.BasicModels;

namespace UserProfileService.Validation.Abstractions.Configuration;

/// <summary>
///     Validation configuration for <see cref="GroupBasic" />
/// </summary>
public class GroupValidationConfiguration
{
    /// <summary>
    ///     Configuration to validate name and display name of group.
    ///     Is also taken into account during sync.
    /// </summary>
    public NameConfiguration Name { get; set; } = new NameConfiguration();
}
