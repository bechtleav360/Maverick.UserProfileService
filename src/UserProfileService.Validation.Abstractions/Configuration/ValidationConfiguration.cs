// TODO: Move configuration to validation package. Split config for saga validation and sync in different classes.

namespace UserProfileService.Validation.Abstractions.Configuration;

/// <summary>
///     Configuration to customize validation for entities.
/// </summary>
public class ValidationConfiguration
{
    /// <summary>
    ///     Validation configuration for each command.
    /// </summary>
    public CommandValidationConfiguration Commands { get; set; } = new CommandValidationConfiguration();

    /// <summary>
    ///     Validation configuration for internal entities.
    /// </summary>
    public EntityConfiguration Internal { get; set; } = new EntityConfiguration();
}
