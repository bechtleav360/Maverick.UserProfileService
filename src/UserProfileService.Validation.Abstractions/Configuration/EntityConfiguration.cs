// TODO: Move configuration to validation package. Split config for saga validation and sync in different classes.

namespace UserProfileService.Validation.Abstractions.Configuration;

/// <summary>
///     Configuration to customize validation for internal entities.
/// </summary>
public class EntityConfiguration
{
    /// <summary>
    ///     Validation configuration for functions.
    /// </summary>
    public FunctionValidationConfiguration Function { get; set; } = new FunctionValidationConfiguration();

    /// <summary>
    ///     Validation configuration for group.
    /// </summary>
    public GroupValidationConfiguration Group { get; set; } = new GroupValidationConfiguration();

    /// <summary>
    ///     Validation configuration for user.
    /// </summary>
    public UserValidationConfiguration User { get; set; } = new UserValidationConfiguration();
}
